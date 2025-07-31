using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands;

public class PerformTransfer
{
    public record Command(
        int FromAccountId,
        int ToAccountId,
        decimal Amount
    ) : IRequest;

    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {

            var fromAccount = await dbContext.Accounts.FindAsync(request.FromAccountId, cancellationToken);
            var toAccount = await dbContext.Accounts.FindAsync(request.ToAccountId, cancellationToken);

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
                DateTime = DateTime.Now
            };

            fromAccount.Balance -= request.Amount;

            var transactionTo = new Transaction()
            {
                AccountId = toAccount.AccountId,
                Amount = request.Amount,
                Currency = toAccount.Currency,
                TransactionType = TransactionType.Debit,
                DateTime = DateTime.Now
            };

            toAccount.Balance += request.Amount;

            dbContext.Accounts.Update(toAccount);
            dbContext.Accounts.Update(fromAccount);
            
            await dbContext.Transactions.AddAsync(transactionFrom, cancellationToken);
            await dbContext.Transactions.AddAsync(transactionTo, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}