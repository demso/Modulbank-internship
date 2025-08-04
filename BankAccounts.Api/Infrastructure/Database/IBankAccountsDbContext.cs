using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Infrastructure.Database;

/// <summary>
/// Интерфейс для бд банковских счетов
/// </summary>
public interface IBankAccountsDbContext
{
    /// <summary>
    /// Банковские счета пользователей
    /// </summary>
    DbSet<Account> Accounts { get; }
    /// <summary>
    /// Транзакции
    /// </summary>
    DbSet<Transaction> Transactions { get; }
    /// <summary>
    /// Сохранение изменений
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}