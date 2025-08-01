using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BankAccounts.Api.Infrastructure;

public class BankAccountsDbContext(DbContextOptions<BankAccountsDbContext> options) : DbContext(options), IBankAccountsDbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("BankAccountDatabase");
    }
}