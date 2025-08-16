using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace BankAccounts.Api.Infrastructure.Repository.Accounts;

/// <summary>
/// Репозиторий для работы со счетами пользователя <see cref="Account"/>.
/// </summary>
/// <param name="dbContext"></param>
public class AccountsRepositoryAsync(IBankAccountsDbContext dbContext, ILogger<AccountsRepositoryAsync> logger) : AbstractRepository(dbContext), IAccountsRepositoryAsync
{

    /// <inheritdoc />
    public async Task<Account?> GetByIdAsync(int accountId, CancellationToken cancellationToken)
    {
        return await DbContext.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Account?> GetByIdWithTransactions(int accountId, CancellationToken cancellationToken)
    {
        return await DbContext.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Account>> GetByFilterAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        IQueryable<Account> query = DbContext.Accounts
            .Where(a => a.OwnerId == ownerId);

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    //todo транзакции ef
    public async Task<Account> AddAsync(Guid ownerId, AccountType accountType, Currencies currency, decimal interestRate,
        Guid causationId, CancellationToken cancellationToken)
    {
        Account account = (await DbContext.Accounts.AddAsync(
            new Account
            {
                OwnerId = ownerId,
                AccountType = accountType,
                Currency = currency,
                InterestRate = interestRate,
                OpenDate = DateTime.UtcNow
            }, cancellationToken
        )).Entity;
        
        await DbContext.SaveChangesAsync(cancellationToken);
    
        return (await GetByIdAsync(account.AccountId, cancellationToken))!;
    }
}