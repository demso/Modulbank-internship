using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.Repository;

/// <summary>
/// Базовый интерфейс для репозиториев сервиса банковских счетов.
/// </summary>
public interface IBankAccountsServiceRepositoryAsync
{
    /// <summary>
    /// Метод для сохранения изменений в базе данных асинхронно.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}