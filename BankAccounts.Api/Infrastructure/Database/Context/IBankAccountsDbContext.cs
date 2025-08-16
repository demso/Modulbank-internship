using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BankAccounts.Api.Infrastructure.Database.Context;

/// <summary>
/// Интерфейс для бд банковских счетов
/// </summary>
public interface IBankAccountsDbContext
{
    /// <summary>
    /// Банковские счета пользователей
    /// </summary>
    DbSet<Account> Accounts { get; }
    /// <summary>
    /// Транзакции
    /// </summary>
    DbSet<Transaction> Transactions { get; }
   
    DbSet<OutboxPublishedEntity> OutboxPublished { get; }
    DbSet<InboxConsumedEntity> InboxConsumed { get; }
    /// <summary>
    /// Сохранение изменений
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// База данных
    /// </summary>
    DatabaseFacade Database { get; }
}