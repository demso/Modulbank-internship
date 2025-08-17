using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankAccounts.Api.Infrastructure.Database.Context;

/// <summary>
/// Для миграций
/// </summary>
public class BankAccountsDbContextFactory : IDesignTimeDbContextFactory<BankAccountsDbContext>
{
    public BankAccountsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankAccountsDbContext>();
        
        // Используйте вашу строку подключения
        var connectionString = "Host=localhost;Database=bankaccounts;Username=postgres;Password=yourpassword";

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MapEnum<Currencies>();
            options.MapEnum<TransactionType>();
            options.MapEnum<AccountType>();
            options.MapEnum<EventType>();
        });
        
        return new BankAccountsDbContext(optionsBuilder.Options);
    }
}
