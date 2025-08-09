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