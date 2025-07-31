using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class CreateAccount
{
    public record Command(
        Guid OwnerId,
        AccountType AccountType,
        string Currency,
        decimal InterestRate
    ) : IRequest<int>;

    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.OwnerId == Guid.Empty)
                throw new Exception("Поле OwnerId в CreateAccount.Command не должно быть пустым Guid.");

            var account = new Account
            {
                OwnerId = request.OwnerId,
                AccountType = request.AccountType,
                Currency = request.Currency,
                InterestRate = request.InterestRate,
                OpenDate = DateTime.Now
            };

            await dbContext.Accounts.AddAsync(account, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return account.AccountId;
        }
    }
}