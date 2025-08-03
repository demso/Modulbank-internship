using AutoMapper;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Transactions.Commands;

public static class PerformTransaction
{
    public record Command : IRequest<TransactionDto>
    {
        public Guid OwnerId { get; set; }
        public int AccountId { get; init; }
        public TransactionType TransactionType { get; init; }
        public decimal Amount { get; init; }
        public string? Description { get; init; }
    }

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Command, TransactionDto>
    {
        public override async Task<TransactionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (request.TransactionType)
            {
                case TransactionType.Debit:
                    account.Balance += request.Amount;
                    break;
                case TransactionType.Credit:
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

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.AccountId).GreaterThan(0);
            RuleFor(command => command.Description).MaximumLength(255);
            RuleFor(command => command.Amount).NotEqual(0);
        }
    }
}