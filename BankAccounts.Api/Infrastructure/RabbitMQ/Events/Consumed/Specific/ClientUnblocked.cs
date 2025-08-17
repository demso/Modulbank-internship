using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Specific
{
    /// <summary>
    /// Сообщение о том, что счета клиента разблокированы
    /// </summary>
    // ReSharper disable once UnusedType.Global Используется
    public class ClientUnblocked : Event
    {
        /// <summary>
        /// Id клиента
        /// </summary>
        // ReSharper disable once UnusedMember.Global Используется
        public Guid ClientId { get; set; }
    }
}
