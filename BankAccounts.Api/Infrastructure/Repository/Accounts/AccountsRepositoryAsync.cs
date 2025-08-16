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
        // Получаем соединение и открываем его при необходимости
        DbConnection connection = DbContext.Database.GetDbConnection();
        bool wasClosed = connection.State == ConnectionState.Closed;

        if (wasClosed)
            await connection.OpenAsync(cancellationToken);

        // Создаём транзакцию с уровнем изоляции Serializable
        await using DbTransaction transaction = 
            await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await DbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        try
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

            AccountOpened accountOpened = new AccountOpened
            {
                AccountId = account.AccountId, 
                OwnerId = ownerId, 
                AccountType = accountType, 
                Currency = currency,
                Metadata = new Metadata
                {
                    CausationId = causationId
                }
            };
            
            OutboxPublishedEntity entity = new() { 
                EventType = EventType.AccountOpened, 
                Message = JsonObjectSerializer.ToJson(accountOpened), 
                Created = accountOpened.OccurredAt 
            };
            
            await DbContext.OutboxPublished.AddAsync(entity, cancellationToken);
            
            await DbContext.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        
            return (await GetByIdAsync(account.AccountId, cancellationToken))!;
        }
        catch (Exception ex)
        {
            const string message = "Account not opened due to an error. ";
            // Откатываем транзакцию
            throw new Exception(message, ex);
        }
        finally
        {
            // Убираем транзакцию из контекста
            await DbContext.Database.UseTransactionAsync(null, cancellationToken);

            // Возвращаем соединение в исходное состояние
            if (wasClosed)
                await connection.CloseAsync();
        }
    }
}