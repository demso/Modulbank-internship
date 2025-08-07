using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;
using FluentValidation;
using MediatR;
// ReSharper disable once UnusedType.Global Класс используется посредником

namespace BankAccounts.Api.Features.Transactions.Commands;

/// <summary>
/// Произвести перевод средств между счетами
/// </summary>
public static class PerformTransfer
{
    /// <summary>
    /// Команда для проведения трансфера
    /// </summary>
    public record Command : IRequest<TransactionDto>
    {
        /// <summary>
        /// Id пользователя
        /// </summary>
        public Guid OwnerId { get; set; }
        /// <summary>
        /// Id исходящего счета
        /// </summary>
        public int FromAccountId { get; init; }
        /// <summary>
        /// Id счета назначения
        /// </summary>
        public int ToAccountId { get; init; }
        /// <summary>
        /// Сумма денежных средств
        /// </summary>
        public decimal Amount { get; init; }
    }

    /// <summary>
    /// Обработчик команды. Трансфер происходит с использованием двух транзакций, одна снимает средства с исходного счета
    /// и одна зачисляет на конченый счет.
    /// </summary>
    public class Handler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, ICurrencyService currencyService, IMapper mapper) : BaseRequestHandler<Command, TransactionDto>
    {
        /// <inheritdoc />
        public override async Task<TransactionDto> Handle(Command request,  CancellationToken cancellationToken)
        {

            var fromAccount = await GetValidAccount(accountsRepository, request.FromAccountId, request.OwnerId, cancellationToken);

            var toAccount = await accountsRepository.GetByIdAsync(request.ToAccountId, cancellationToken);

            if (toAccount is null)
                throw new AccountNotFoundException(request.ToAccountId);

            var transactionFrom = await transactionsRepository.AddAsync(
                fromAccount.AccountId,
                toAccount.AccountId,
                request.Amount,
                fromAccount.Currency,
                TransactionType.Credit,
               $"Transaction from {fromAccount.AccountId} account to {toAccount.AccountId}.", cancellationToken);

            fromAccount.Balance -= request.Amount;
            // Если необходимо, конвертируем валюту в валюту конечного счета
            var toAccountAmount = currencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency);

            await transactionsRepository.AddAsync(
                toAccount.AccountId,
                fromAccount.AccountId,
                toAccountAmount,
                 toAccount.Currency,
                TransactionType.Debit,
                $"Transaction to {toAccount.AccountId} account from {fromAccount.AccountId}.", cancellationToken);

            toAccount.Balance += toAccountAmount;

            await transactionsRepository.SaveChangesAsync(cancellationToken);
            await accountsRepository.SaveChangesAsync(cancellationToken);

            return mapper.Map<TransactionDto>(transactionFrom);
        }
    }
    /// <summary>
    /// Валидатор команды
    /// </summary>
    public class CommandValidator : AbstractValidator<Command>
    {
        
        /// <inheritdoc />
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.FromAccountId).GreaterThan(0).NotEqual(command => command.ToAccountId);
            RuleFor(command => command.ToAccountId).GreaterThan(0);
            RuleFor(command => command.Amount).GreaterThan(0);
        }
    }
}