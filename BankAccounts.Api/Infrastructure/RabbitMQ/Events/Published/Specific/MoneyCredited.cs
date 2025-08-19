using BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific
{
    /// <summary>
    /// Событие, вызываемое в случае начисления средств на счет <seealso cref="PerformTransactionHandler"/>
    /// </summary>
    public class MoneyCredited : Event
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Id счета
        /// </summary>
        public int? AccountId { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Количество средств
        /// </summary>
        public decimal? Amount { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Валюта
        /// </summary>
        public  Currencies? Currency { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        /// <summary>
        /// Id операции
        /// </summary>
        public Guid? OperationId { get; set; }
    }
}

