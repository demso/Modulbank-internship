using BankAccounts.Api.Infrastructure.Hangfire.Jobs;
using Hangfire;

namespace BankAccounts.Api.Infrastructure.Hangfire.Registerer;

public class JobsRegistrator
    : AbstractJobsRegistrator
{
    protected override void AddJobs()
    {
        RecurringJob.AddOrUpdate<AccrueInterestJob>("accrueInterest", obj =>
                obj.Job()
            , Cron.Daily);
    }
}