using Hangfire.Dashboard;

namespace BankAccounts.Api.Infrastructure.Hangfire;


public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Для просмотра HangfireDashboard не нужна авторизация
        return true;
    }
}