using BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific
{
    /// <summary>
    /// Событие, вызываемое в случае проведения трансфера <seealso cref="PerformTransferHandler"/>
    /// </summary>
    public class TransferCompleted : Event
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Id счета источника
        /// </summary>
        public int? SourceAccountId { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Id счета назначения
        /// </summary>
        public int? DestinationAccountId { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Количество средств
        /// </summary>
        public decimal? Amount { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Валюта
        /// </summary>
        public Currencies? Currency { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Id трансфера
        /// </summary>
        public Guid? TransferId { get; set; }
    }
}
