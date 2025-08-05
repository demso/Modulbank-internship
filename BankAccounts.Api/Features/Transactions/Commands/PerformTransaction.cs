using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.Database;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands;

/// <summary>
/// Провести транзакцию со счетом
/// </summary>
public static class PerformTransaction
{
    /// <summary>
    /// Команда для проведения транзакции
    /// </summary>
    public record Command : IRequest<TransactionDto>
    {
        /// <summary>
        /// Id владельца счета
        /// </summary>
        public Guid OwnerId { get; set; }
        /// <summary>
        /// Id счета
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// Тип транзакции
        /// </summary>
        public TransactionType TransactionType { get; init; }
        /// <summary>
        /// Сумма денежных средств
        /// </summary>
        public decimal Amount { get; init; }
        /// <summary>
        /// Описание транзакции
        /// </summary>
        public string? Description { get; init; }
    }

    /// <summary>
    /// Обработчик команды
    /// </summary>
    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Command, TransactionDto>
    {
        /// <inheritdoc />
        public override async Task<TransactionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault Решарпер предлагает непонятный код
            switch (request.TransactionType)
            {
                case TransactionType.Debit:
                    account.Balance += request.Amount;
                    break;
                case TransactionType.Credit:
                    if (account.AccountType is AccountType.Checking or AccountType.Deposit &&
                        account.Balance - request.Amount < 0)
                        throw new BadRequestException("Баланс после транзакции не может быть < 0, т.к. счет не является кредитным.");
                    account.Balance -= request.Amount;
                    break;
            }

            var transaction = new Transaction
            {
                AccountId = account.AccountId,
                Amount = request.Amount,
                Currency = account.Currency,
                TransactionType = request.TransactionType,
                DateTime = DateTime.Now,
                Description = request.Description
            };

            await dbDbContext.Transactions.AddAsync(transaction, cancellationToken);
            await dbDbContext.SaveChangesAsync(cancellationToken);

            return mapper.Map<TransactionDto>(transaction);
        }
    }
    /// <summary>
    /// Валидатор команды
    /// </summary>
    // ReSharper disable once UnusedType.Global Класс используется посредником
    public class CommandValidator : AbstractValidator<Command>
    {
        /// <summary>
        /// Создание валидатора и задание правил валидации
        /// </summary>
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.AccountId).GreaterThan(0);
            RuleFor(command => command.Description).MaximumLength(255);
            RuleFor(command => command.Amount).GreaterThan(0);
        }
    }
}