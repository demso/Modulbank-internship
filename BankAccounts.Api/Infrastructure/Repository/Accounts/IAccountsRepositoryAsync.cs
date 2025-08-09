using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;

namespace BankAccounts.Api.Infrastructure.Repository.Accounts;

/// <summary>
/// Интерфейс репозитория для работы с банковскими счетами.
/// </summary>
public interface IAccountsRepositoryAsync : IBankAccountsServiceRepositoryAsync
{
    /// <summary>
    /// Получить счет по идентификатору.
    /// </summary>
    Task<Account?> GetByIdAsync(int accountId, CancellationToken cancellationToken);
    /// <summary>
    /// Получить список счетов по фильтру владельца.
    /// </summary>
    Task<List<Account>> GetByFilterAsync(Guid ownerId, CancellationToken cancellationToken);
    /// <summary>
    /// Польучить счет по идентификатору с транзакциями.
    /// </summary>
    Task<Account?> GetByIdWithTransactions(int accountId, CancellationToken cancellationToken);
    /// <summary>
    /// Метод возвращает список счетов владельца с пагинацией.
    /// </summary>
    Task<List<Account>> GetByOwnerByPageAsync(Guid ownerId, int size, int pageSize, CancellationToken cancellationToken);
    /// <summary>
    /// Добавить новый счет.
    /// </summary>
    Task<Account> AddAsync(Guid ownerId, AccountType accountType, Currencies currency, decimal interestRate, CancellationToken cancellationToken);
}