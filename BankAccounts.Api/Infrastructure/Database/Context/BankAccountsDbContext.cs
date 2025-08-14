using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}