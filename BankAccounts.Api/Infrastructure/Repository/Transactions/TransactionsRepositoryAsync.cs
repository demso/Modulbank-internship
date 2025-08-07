using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Repository.Transactions;

public class TransactionsRepositoryAsync(IBankAccountsDbContext dbContext) : ITransactionsRepositoryAsync
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        return await dbContext.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
    }

    public async Task<List<Transaction>> GetByFilterAsync(int accountId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        return await dbContext.Transactions
            .Where(t => t.AccountId == accountId &&
                        (!from.HasValue || DateOnly.FromDateTime(t.DateTime.ToUniversalTime()) >= from.Value)
                        && (!to.HasValue || DateOnly.FromDateTime(t.DateTime.ToUniversalTime()) <= to.Value))
            .OrderByDescending(t => t.DateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByAccountByPageAsync(int accountId, DateOnly? from, DateOnly? to, int page, int pageSize, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Transactions
            .Where(t => t.AccountId == accountId &&
                        (!from.HasValue || DateOnly.FromDateTime(t.DateTime.ToUniversalTime()) >= from.Value) 
                        && (!to.HasValue || DateOnly.FromDateTime(t.DateTime.ToUniversalTime()) <= to.Value))
            .OrderByDescending(t => t.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction> AddAsync(int accountId, int counterPartyId, decimal amount, Currencies currency, TransactionType transactionType,
        string? description, CancellationToken cancellationToken)
    {
        return (await dbContext.Transactions.AddAsync
                (
                    new Transaction
                    {
                        AccountId = accountId,
                        CounterpartyAccountId = counterPartyId,
                        Amount = amount,
                        Currency = currency,
                        TransactionType = transactionType,
                        Description = description,
                        DateTime = DateTime.UtcNow
                    },
                    cancellationToken
                )
            )
            .Entity;
    }
}