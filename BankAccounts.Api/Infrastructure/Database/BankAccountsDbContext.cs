using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Database;

/// <summary>
/// База данных для банковских счетов и транзакций
/// </summary>
public class BankAccountsDbContext(DbContextOptions<BankAccountsDbContext> options) : DbContext(options), IBankAccountsDbContext
{
    /// <summary>
    /// Банковские счета пользователей
    /// </summary>
    public DbSet<Account> Accounts => Set<Account>();
    /// <summary>
    /// Транзакции
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();
    /// <summary>
    /// Конфигурация
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("BankAccountDatabase");
    }
}