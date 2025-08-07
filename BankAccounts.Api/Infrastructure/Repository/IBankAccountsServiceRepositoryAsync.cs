namespace BankAccounts.Api.Infrastructure.Repository;

public interface IBankAccountsServiceRepositoryAsync
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}