namespace BankAccounts.Api.Infrastructure.Hangfire.Jobs;

public interface IJob
{
    Task Job();
}