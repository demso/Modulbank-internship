using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankAccounts.Api.Infrastructure.Database.Context
{
    /// <summary>
    /// Для миграций
    /// </summary>
// ReSharper disable once UnusedType.Global Используется при создании миграций
    public class BankAccountsDbContextFactory : IDesignTimeDbContextFactory<BankAccountsDbContext>
    {
        /// <summary>
        /// Создаст контекст
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public BankAccountsDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<BankAccountsDbContext> optionsBuilder = new();
        
            const string connectionString = "Host=localhost;Database=bankaccounts;Username=postgres;Password=password";

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
}
