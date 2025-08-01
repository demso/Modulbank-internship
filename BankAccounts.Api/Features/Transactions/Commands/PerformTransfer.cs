using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands;

public class PerformTransfer
{
    public record Command(
        int FromAccountId,
        int ToAccountId,
        decimal Amount
    ) : IRequest<TransactionDto>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : IRequestHandler<Command, TransactionDto>
    {
        public async Task<TransactionDto> Handle(Command request, CancellationToken cancellationToken)
        {

            var fromAccount = await dbDbContext.Accounts.FindAsync(request.FromAccountId, cancellationToken);
            var toAccount = await dbDbContext.Accounts.FindAsync(request.ToAccountId, cancellationToken);

            if (fromAccount is null)
                throw new NotFoundException(nameof(Account), request.FromAccountId);
            if (toAccount is null)
                throw new NotFoundException(nameof(Account), request.ToAccountId);

            if (request.Amount <= 0)
                throw new Exception("Количество переводимых средств должно быть больше нуля.");

            var transactionFrom = new Transaction()
            {
                AccountId = fromAccount.AccountId,
                Amount = request.Amount,
                Currency = fromAccount.Currency,
                TransactionType = TransactionType.Credit,
                DateTime = DateTime.Now,
                Description = $"Transaction from {fromAccount.AccountId} account."
            };

            fromAccount.Balance -= request.Amount;

            var transactionTo = new Transaction()
            {
                AccountId = toAccount.AccountId,
                Amount = request.Amount,
                Currency = toAccount.Currency,
                TransactionType = TransactionType.Debit,
                DateTime = DateTime.Now,
                Description = $"Transaction to {toAccount.AccountId} account."
            };

            toAccount.Balance += CurrencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency);

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
            RuleFor(command => command.FromAccountId).GreaterThan(0).NotEqual(command => command.ToAccountId);
            RuleFor(command => command.ToAccountId).GreaterThan(0);
            RuleFor(command => command.Amount).GreaterThan(0);
        }
    }
}