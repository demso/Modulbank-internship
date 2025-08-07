using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;

namespace BankAccounts.Api.Infrastructure.Repository.Accounts;

public interface IAccountsRepositoryAsync : IBankAccountsServiceRepositoryAsync
{
    Task<Account?> GetByIdAsync(int accountId, CancellationToken cancellationToken);
    Task<List<Account>> GetByFilterAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<List<Account>> GetByOwnerByPageAsync(Guid ownerId, int size, int pageSize, CancellationToken cancellationToken);
    Task<Account> AddAsync(Guid ownerId, AccountType accountType, Currencies currency, decimal interestRate, CancellationToken cancellationToken);
}


//Task<List<Account>> Get();
//Task<List<Account>> GetWithTransactions();