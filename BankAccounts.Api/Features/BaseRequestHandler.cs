using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features;

public abstract class BaseRequestHandler<TRequest, TResponse>
     : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

    protected async Task<Account> GetValidAccount(IBankAccountsDbContext dbContext, int accountId, Guid ownerId, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FindAsync([accountId], cancellationToken);

        if (account == null || account.OwnerId != ownerId)
            throw new AccountNotFoundException(accountId);
        
        return account;
    }
}