namespace BankAccounts.Api.Infrastructure.Hangfire.Jobs;

/// <summary>
/// Интрефейс для фоновых заданий Hangfire.
/// </summary>
public interface IJob
{
    /// <summary>
    /// Задание для выполнения в фоновом режиме.
    /// </summary>
    Task Job();
}