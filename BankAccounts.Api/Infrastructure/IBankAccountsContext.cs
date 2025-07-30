using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure;

public interface IBankAccountsContext
{
    DbSet<Account> Accounts { get; set; }
    DbSet<Transaction> Transactions { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}