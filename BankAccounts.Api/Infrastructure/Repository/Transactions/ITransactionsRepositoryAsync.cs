using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BankAccounts.Api.Infrastructure.Repository.Transactions;

public interface ITransactionsRepositoryAsync : IBankAccountsServiceRepositoryAsync
{
    Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken);
    Task<List<Transaction>> GetByFilterAsync(int accountId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
    Task<List<Transaction>> GetByAccountByPageAsync(int accountId, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken cancellationToken);
    Task<Transaction> AddAsync(int accountId, int counterPartyId, decimal amount, Currencies currency, 
        TransactionType transactionType, string? description,  CancellationToken cancellationToken);
}