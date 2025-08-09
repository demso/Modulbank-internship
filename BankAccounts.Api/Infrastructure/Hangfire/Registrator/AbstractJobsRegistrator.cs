namespace BankAccounts.Api.Infrastructure.Hangfire.Registerer;

public abstract class AbstractJobsRegistrator : IHostedService
{
    protected abstract void AddJobs();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AddJobs();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}