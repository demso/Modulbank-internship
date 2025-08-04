using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.CurrencyService;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

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
    public class Handler(IBankAccountsDbContext dbDbContext, ICurrencyService currencyService, IMapper mapper) : BaseRequestHandler<Command, TransactionDto>
    {
        /// <inheritdoc />
        public override async Task<TransactionDto> Handle(Command request,  CancellationToken cancellationToken)
        {

            var fromAccount = await GetValidAccount(dbDbContext, request.FromAccountId, request.OwnerId, cancellationToken);

            var toAccount = await dbDbContext.Accounts.FindAsync([request.ToAccountId], cancellationToken);

            if (toAccount is null)
                throw new AccountNotFoundException(request.ToAccountId);
            
            var transactionFrom = new Transaction
            {
                AccountId = fromAccount.AccountId,
                Amount = request.Amount,
                Currency = fromAccount.Currency,
                TransactionType = TransactionType.Credit,
                DateTime = DateTime.Now,
                Description = $"Transaction from {fromAccount.AccountId} account."
            };

            fromAccount.Balance -= request.Amount;
            // Если необходимо, конвертируем валюту в валюту конечного счета
            var toAccountAmount = currencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency);

            var transactionTo = new Transaction
            {
                AccountId = toAccount.AccountId,
                Amount = toAccountAmount,
                Currency = toAccount.Currency,
                TransactionType = TransactionType.Debit,
                DateTime = DateTime.Now,
                Description = $"Transaction to {toAccount.AccountId} account."
            };

            toAccount.Balance += toAccountAmount;

            dbDbContext.Accounts.Update(fromAccount);
            dbDbContext.Accounts.Update(toAccount);
            
            await dbDbContext.Transactions.AddAsync(transactionFrom, cancellationToken);
            await dbDbContext.Transactions.AddAsync(transactionTo, cancellationToken);
            await dbDbContext.SaveChangesAsync(cancellationToken);

            return mapper.Map<TransactionDto>(transactionFrom);
        }
    }
    /// <summary>
    /// Валидатор команды
    /// </summary>
    // ReSharper disable once UnusedType.Global Класс используется посредником
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