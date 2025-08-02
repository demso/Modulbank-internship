using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Transactions.Commands;

public static class PerformTransfer
{
    public record Command : IRequest<TransactionDto>
    {
        public Guid OwnerId { get; set; }
        public int FromAccountId { get; init; }
        public int ToAccountId { get; init; }
        public decimal Amount { get; init; }
    }

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Command, TransactionDto>
    {
        public override async Task<TransactionDto> Handle(Command request, CancellationToken cancellationToken)
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

            var toAccountAmount = CurrencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency);

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

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.FromAccountId).GreaterThan(0).NotEqual(command => command.ToAccountId);
            RuleFor(command => command.ToAccountId).GreaterThan(0);
            RuleFor(command => command.Amount).GreaterThan(0);
        }
    }
}