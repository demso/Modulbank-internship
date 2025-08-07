using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Repository.Accounts;

public class AccountsRepositoryAsync(IBankAccountsDbContext dbContext) : IAccountsRepositoryAsync
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(int accountId, CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    }

    public async Task<Account?> GetByIdWithTransactions(int accountId, CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    }

    public async Task<List<Account>> GetByFilterAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var query = dbContext.Accounts
            .Where(a => a.OwnerId == ownerId);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Account>> GetByOwnerByPageAsync(Guid ownerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .OrderBy(a => a.AccountId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    
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



//public async Task<List<Account>> Get()
//{
//    return await dbContext.Accounts
//        .OrderBy(a => a.AccountId)
//        .AsNoTracking()
//        .ToListAsync();
//}

//public async Task<List<Account>> GetWithTransactions()
//{
//    return await dbContext.Accounts
//        .AsNoTracking()
//        .OrderBy(a => a.AccountId)
//        .Include(a => a.Transactions)
//        .ToListAsync();
//}