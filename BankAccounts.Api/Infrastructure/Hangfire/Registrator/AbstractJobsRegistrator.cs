namespace BankAccounts.Api.Infrastructure.Hangfire.Registerer;

/// <summary>
/// Абстрактный класс для классов регистрации фоновых заданий Hangfire.
/// </summary>
public abstract class AbstractJobsRegistrator : IHostedService
{
    /// <summary>
    /// Метод для добавления фоновых заданий Hangfire.
    /// </summary>
    protected abstract void AddJobs();

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        AddJobs();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}