using Hangfire.Dashboard;

namespace BankAccounts.Api.Infrastructure.Hangfire
{
    /// <summary>
    /// Фильтр авторизации для Hangfire Dashboard, отключающий необходимость авторизации (для удобного тестирования).
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Метод авторизации для Hangfire Dashboard.
        /// </summary>
        public bool Authorize(DashboardContext context)
        {
            // Для просмотра HangfireDashboard не нужна авторизация
            return true;
        }
    }
}