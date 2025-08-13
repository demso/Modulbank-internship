using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using IsolationLevel = System.Data.IsolationLevel;

// ReSharper disable once UnusedType.Global Класс используется посредником

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;

/// <summary>
/// Обработчик команды <see cref="PerformTransferCommand"/> Трансфер происходит с использованием двух транзакций,
/// одна снимает средства с исходного счета и одна зачисляет на конечный счет.
/// </summary>
public class PerformTransferHandler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, 
    IBankAccountsDbContext dbContext, ICurrencyService currencyService, IMapper mapper, 
    ILogger<PerformTransferHandler> logger) : RequestHandlerBase<PerformTransferCommand, TransactionDto>
{
    /// <inheritdoc />
    public override async Task<TransactionDto> Handle(PerformTransferCommand request,  CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Начало перевода {Amount} со счёта {FromAccountId} на счёт {ToAccountId}",
            request.Amount, request.FromAccountId, request.ToAccountId);

        // Получаем соединение и открываем его при необходимости
        var connection = dbContext.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;

        if (wasClosed)
            await connection.OpenAsync(cancellationToken);

        // Создаём транзакцию с уровнем изоляции Serializable
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await dbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        try
        {
            var fromAccount = await GetValidAccount(accountsRepository, request.FromAccountId, request.OwnerId, cancellationToken);
            var toAccount = await accountsRepository.GetByIdAsync(request.ToAccountId, cancellationToken);
            
            if (toAccount is null)
                throw new AccountNotFoundException(request.ToAccountId);

            // Проверяем достаточность средств
            if (fromAccount.Balance < request.Amount && !fromAccount.AccountType.Equals(AccountType.Credit))
                throw new BadRequestException("Недостаточно средств на счёте");

            var toAccountAmount = currencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency);

            // Выполняем перевод
            fromAccount.Balance -= request.Amount;
            toAccount.Balance += toAccountAmount;
            // Транзакция с первого счета
            var transactionFrom = await transactionsRepository.AddAsync(
                fromAccount.AccountId,
                toAccount.AccountId,
                request.Amount,
                fromAccount.Currency,
                TransactionType.Credit,
                $"Перевод со счета {fromAccount.AccountId} на счет {toAccount.AccountId}.", cancellationToken);
            // Транзакция на второй счет
            await transactionsRepository.AddAsync(
                toAccount.AccountId,
                fromAccount.AccountId,
                toAccountAmount,
                toAccount.Currency,
                TransactionType.Debit,
                $"Перевод на счет {toAccount.AccountId} со счета {fromAccount.AccountId}.", cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            // Сохраняем ожидаемые значения балансов для проверки
            var expectedFromBalance = fromAccount.Balance;
            var expectedToBalance = toAccount.Balance;

            // Получаем значения для проверки 
            var updatedFromAccount = await accountsRepository.GetByIdAsync(request.FromAccountId, cancellationToken);
            var updatedToAccount = await accountsRepository.GetByIdAsync(request.ToAccountId, cancellationToken);

            // Проверяем соответствие балансов
            if (updatedFromAccount?.Balance != expectedFromBalance ||
                updatedToAccount?.Balance != expectedToBalance)
            {
                logger.LogWarning(
                    "Несоответствие балансов после перевода. Откат транзакции. " +
                    "Ожидалось: From={ExpectedFrom}, To={ExpectedTo}. " +
                    "Фактически: From={ActualFrom}, To={ActualTo}",
                    expectedFromBalance, expectedToBalance,
                    updatedFromAccount?.Balance, updatedToAccount?.Balance);

                // Откатываем транзакцию
                throw new BadRequestException("Несоответствие балансов. Транзакция отменена.");
            }

            // Подтверждаем транзакцию
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Перевод успешно выполнен. Счёт {FromAccountId}: {FromBalance}, Счёт {ToAccountId}: {ToBalance}",
                request.FromAccountId, updatedFromAccount.Balance, request.ToAccountId, updatedToAccount.Balance);

            return mapper.Map<TransactionDto>(transactionFrom);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex,
                "Конфликт параллельного обновления при переводе средств со счёта {FromAccountId} на счёт {ToAccountId}",
                request.FromAccountId, request.ToAccountId);
            // Откатываем транзакцию
            throw new ConcurrencyException(
                "Запись была изменена другим пользователем. Пожалуйста, повторите операцию.",
                ex);
        }
        catch (Exception)
        {
            logger.LogError("Ошибка при переводе средств со счёта {FromAccountId} на счёт {ToAccountId}. Транзакция отменена.",
                request.FromAccountId, request.ToAccountId);

            // Откатываем транзакцию
            throw;
        }
        finally
        {
            // Убираем транзакцию из контекста
            await dbContext.Database.UseTransactionAsync(null, cancellationToken);

            // Возвращаем соединение в исходное состояние
            if (wasClosed)
                await connection.CloseAsync();
        }
    }
}