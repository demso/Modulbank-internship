using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BankAccounts.Api.Infrastructure.Database.Context;

/// <summary>
/// Контекст для базы данных банковских счетов и транзакций
/// </summary>
public sealed class BankAccountsDbContext(DbContextOptions<BankAccountsDbContext> options) : DbContext(options), IBankAccountsDbContext
{
    /// <summary>
    /// Банковские счета пользователей (<see cref="Account"/>)
    /// </summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>
    /// Транзакции (<see cref="Transaction"/>)
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <inheritdoc />
    public DbSet<OutboxPublishedEntity>  OutboxPublished => Set<OutboxPublishedEntity>();

    /// <inheritdoc />
    public DbSet<InboxConsumedEntity> InboxConsumed => Set<InboxConsumedEntity>();

    /// <inheritdoc />
    public DbSet<DeadLetterEntity> DeadLetters => Set<DeadLetterEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}