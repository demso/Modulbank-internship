using BankAccounts.Api.Infrastructure.Hangfire.Jobs;
using Hangfire;

namespace BankAccounts.Api.Infrastructure.Hangfire.Registrator;

/// <summary>
/// Конкретный класс для регистрации ежедневного начисления процентов.
/// </summary>
public class JobsRegistrator(IServiceProvider services) : AbstractJobsRegistrator
{
    /// <inheritdoc />
    protected override void AddJobs()
    {
        RecurringJob.AddOrUpdate<AccrueInterestJob>("accrueInterest", obj => obj.Job(), Cron.Daily);
        RecurringJob.AddOrUpdate<Sender>("send_events", obj => obj.Job(), "*/10 * * * * *");
    }
}