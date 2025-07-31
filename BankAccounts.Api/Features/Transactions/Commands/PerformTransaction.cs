using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands;

public class PerformTransaction
{
    public record Command(
        Guid OwnerId,
        int AccountId,
        TransactionType TransactionType,
        decimal Amount
    ) : IRequest<Guid>;

    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.OwnerId == Guid.Empty)
                throw new Exception("Id пользователя не должен быть пустым Guid.");
            
            var account = await dbContext.Accounts.FindAsync(request.AccountId, cancellationToken);
            if (account is null)
                throw new Exception($"Счет с id = {request.AccountId} не найден.");
            if (account.OwnerId != request.OwnerId)
                throw new Exception("Пользователь должен быть владельцем счета.");

            if (request.Amount == 0)
                throw new Exception("Количество переводимых средств должно быть ненулевой суммой.");

            account.Balance += request.Amount;

            var transaction = new Transaction()
            {
                AccountId = account.AccountId,
                Amount = request.Amount,
                Currency = account.Currency,
                DateTime = DateTime.Now
            };

            await dbContext.Transactions.AddAsync(transaction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return transaction.TransactionId;
        }
    }
}