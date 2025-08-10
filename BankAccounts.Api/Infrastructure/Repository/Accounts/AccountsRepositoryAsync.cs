using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Repository.Accounts;

/// <summary>
/// Репозиторий для работы сос счетами пользователя.
/// </summary>
/// <param name="dbContext"></param>
public class AccountsRepositoryAsync(IBankAccountsDbContext dbContext) : IAccountsRepositoryAsync
{
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Account?> GetByIdAsync(int accountId, CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Account?> GetByIdWithTransactions(int accountId, CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Account>> GetByFilterAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var query = dbContext.Accounts
            .Where(a => a.OwnerId == ownerId);

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Account> AddAsync(Guid ownerId, AccountType accountType, Currencies currency, decimal interestRate, CancellationToken cancellationToken)
    {
        return (await dbContext.Accounts.AddAsync(new Account
                {
                    OwnerId = ownerId,
                    AccountType = accountType,
                    Currency = currency,
                    InterestRate = interestRate,
                    OpenDate = DateTime.UtcNow
                }, cancellationToken))
            .Entity;
    }
}