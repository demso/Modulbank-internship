using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands;

public class PerformTransaction
{
    public record Command(
        int AccountId,
        TransactionType TransactionType,
        decimal Amount
    ) : IRequest<Guid>;

    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            
            var account = await dbContext.Accounts.FindAsync(request.AccountId, cancellationToken);
            if (account is null)
                throw new Exception($"Счет с id = {request.AccountId} не найден.");

            if (request.Amount <= 0)
                throw new Exception("Количество переводимых средств должно быть больше нуля.");

            if (request.TransactionType == TransactionType.Debit)
                account.Balance += request.Amount;
            if (request.TransactionType == TransactionType.Credit)
                account.Balance -= request.Amount;

            var transaction = new Transaction()
            {
                AccountId = account.AccountId,
                Amount = request.Amount,
                Currency = account.Currency,
                TransactionType = request.TransactionType,
                DateTime = DateTime.Now
            };

            await dbContext.Transactions.AddAsync(transaction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return transaction.TransactionId;
        }
    }
}