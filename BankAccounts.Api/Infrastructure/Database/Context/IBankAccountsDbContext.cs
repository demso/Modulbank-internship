using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BankAccounts.Api.Infrastructure.Database.Context;

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
    /// <summary>
    /// База данных
    /// </summary>
    DatabaseFacade Database { get; }
}