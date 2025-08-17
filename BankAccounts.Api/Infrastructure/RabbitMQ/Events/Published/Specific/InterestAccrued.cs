using BankAccounts.Api.Infrastructure.Hangfire.Jobs;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific
{
    /// <summary>
    /// Событие, вызываемое при начислении процентов <seealso cref="AccrueInterestJob"/>
    /// </summary>
    public class InterestAccrued : Event
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// ID счета
        /// </summary>
        public int? AccountId { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Начало периода
        /// </summary>
        public DateOnly? PeriodFrom { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Конец периода
        /// </summary>
        public DateOnly? PeriodTo { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Количество средств
        /// </summary>
        public decimal? Amount { get; set; }
    }
}
