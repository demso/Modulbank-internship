using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.EntityTypeConfiguration;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Database.Context;

/// <summary>
/// Контекст для базы данных банковских счетов и транзакций
/// </summary>
public sealed class BankAccountsDbContext(IConfiguration configuration) : DbContext, IBankAccountsDbContext
{
    /// <summary>
    /// Банковские счета пользователей
    /// </summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>
    /// Транзакции
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString(nameof(BankAccountsDbContext)), options =>
        {
            options.MapEnum<Currencies>();
            options.MapEnum<TransactionType>();
            options.MapEnum<AccountType>();
        });
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
    }
}