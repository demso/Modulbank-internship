using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Repository.Transactions;

/// <summary>
/// Конкретный класс репозитория для работы с транзакциями <see cref="Transaction"/>.
/// </summary>
public class TransactionsRepositoryAsync(IBankAccountsDbContext dbContext) : AbstractRepository(dbContext), ITransactionsRepositoryAsync
{
    /// <inheritdoc />
    public async Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        return await DbContext.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Transaction>> GetByFilterAsync(int accountId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        return await DbContext.Transactions
            .Where(t => t.AccountId == accountId &&
                        (!from.HasValue || DateOnly.FromDateTime(t.DateTime.ToUniversalTime()) >= from.Value)
                        && (!to.HasValue || DateOnly.FromDateTime(t.DateTime.ToUniversalTime()) <= to.Value))
            .OrderByDescending(t => t.DateTime)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Transaction> AddAsync(int accountId, int counterPartyId, decimal amount, Currencies currency, TransactionType transactionType,
        string? description, CancellationToken cancellationToken)
    {
        return (await DbContext.Transactions.AddAsync
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