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
using System.Data.Common;
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
        LogBegin(request);

        // Получаем соединение и открываем его при необходимости
        DbConnection connection = dbContext.Database.GetDbConnection();
        bool wasClosed = connection.State == ConnectionState.Closed;

        if (wasClosed)
            await connection.OpenAsync(cancellationToken);

        // Создаём транзакцию с уровнем изоляции Serializable
        await using DbTransaction transaction = 
            await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await dbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        try
        {
            Account fromAccount = 
                await GetValidAccount(accountsRepository, request.FromAccountId, request.OwnerId, cancellationToken);
            Account? toAccount = await accountsRepository.GetByIdAsync(request.ToAccountId, cancellationToken);
            
            if (toAccount is null)
                throw new AccountNotFoundException(request.ToAccountId);

            // Проверяем достаточность средств
            CheckBalance(request, fromAccount);

            decimal toAccountAmount = currencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency);

            // Выполняем перевод
            fromAccount.Balance -= request.Amount;
            toAccount.Balance += toAccountAmount;
            // Транзакция с первого счета
            Transaction transactionFrom = await transactionsRepository.AddAsync(
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

            // Получаем значения для проверки 
            Account updatedFromAccount = (await accountsRepository.GetByIdAsync(request.FromAccountId, cancellationToken))!;
            Account updatedToAccount = (await accountsRepository.GetByIdAsync(request.ToAccountId, cancellationToken))!;

            // Проверяем соответствие балансов
            CheckBalanceCompliance(updatedFromAccount, fromAccount, updatedToAccount, toAccount);

            // Подтверждаем транзакцию
            await transaction.CommitAsync(cancellationToken);
            
            LogSuccess(request,  updatedFromAccount, updatedToAccount);

            return mapper.Map<TransactionDto>(transactionFrom);
        }
        catch (Exception ex)
        {
             string message = $"Transfer error from {request.FromAccountId} to {request.ToAccountId}." +
                              $" Transaction canceled.";

             // Откатываем транзакцию
            throw new TransferException(message, ex);
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

    private static void CheckBalance(PerformTransferCommand request, Account fromAccount)
    {
        if (fromAccount.Balance < request.Amount && !fromAccount.AccountType.Equals(AccountType.Credit))
            throw new BadRequestException("Недостаточно средств на счёте");
    }

    private void CheckBalanceCompliance(Account updatedFromAccount, Account fromAccount, 
        Account updatedToAccount, Account toAccount)
    {
        // Сохраняем ожидаемые значения балансов для проверки
        decimal expectedFromBalance = fromAccount.Balance;
        decimal expectedToBalance = toAccount.Balance;
        
        if (updatedFromAccount.Balance == expectedFromBalance &&
            updatedToAccount.Balance == expectedToBalance)
        {
            return;
        }

        logger.LogWarning(
            "Несоответствие балансов после перевода. Откат транзакции. " +
            "Ожидалось: From={ExpectedFrom}, To={ExpectedTo}. " +
            "Фактически: From={ActualFrom}, To={ActualTo}",
            expectedFromBalance, expectedToBalance,
            updatedFromAccount, updatedToAccount);

        // Откатываем транзакцию
        throw new BadRequestException("Несоответствие балансов. Транзакция отменена.");

    }

    private void LogBegin(PerformTransferCommand request)
    {
        logger.LogInformation(
            "Начало перевода {Amount} со счёта {FromAccountId} на счёт {ToAccountId}",
            request.Amount, request.FromAccountId, request.ToAccountId);
    }

    private void LogSuccess(PerformTransferCommand request, Account updatedFromAccount, Account updatedToAccount)
    {
        logger.LogInformation(
            "Перевод успешно выполнен. Счёт {FromAccountId}: {FromBalance}, Счёт {ToAccountId}: {ToBalance}",
            request.FromAccountId, updatedFromAccount.Balance, request.ToAccountId, updatedToAccount.Balance);
    }
}