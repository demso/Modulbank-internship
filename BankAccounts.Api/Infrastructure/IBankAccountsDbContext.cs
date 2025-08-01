using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure;

public interface IBankAccountsDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}