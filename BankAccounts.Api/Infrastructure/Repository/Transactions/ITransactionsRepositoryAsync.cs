using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;

namespace BankAccounts.Api.Infrastructure.Repository.Transactions;

/// <summary>
/// Интерфейс репозитория для работы с транзакциями банковских счетов <see cref="Transaction"/>.
/// </summary>
public interface ITransactionsRepositoryAsync : IBankAccountsServiceRepositoryAsync
{
    /// <summary>
    /// Получить транзакцию по идентификатору.
    /// </summary>
    Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken);
    /// <summary>
    /// Получить транзакции по фильтру для конкретного счета.
    /// </summary>
    Task<List<Transaction>> GetByFilterAsync(int accountId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
    /// <summary>
    /// Добавить новую транзакцию в счет асинхронно.
    /// </summary>
    Task<Transaction> AddAsync(int accountId, int counterPartyId, decimal amount, Currencies currency, 
        TransactionType transactionType, string? description,  CancellationToken cancellationToken);
}