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
    ) : IRequest<Guid>;

    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
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