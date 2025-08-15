using BankAccounts.Api.Common;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.Repository
{
    public abstract class AbstractRepository(IBankAccountsDbContext dbContext) : IBankAccountsServiceRepositoryAsync
    {
        private protected readonly IBankAccountsDbContext DbContext = dbContext;
        
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
