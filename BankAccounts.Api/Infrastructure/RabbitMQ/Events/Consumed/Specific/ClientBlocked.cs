using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Specific
{
    /// <summary>
    /// Сообщение о том, что счета клиента заблокированы
    /// </summary>
    // ReSharper disable once UnusedType.Global Используется
    public class ClientBlocked : Event
    {
        /// <summary>
        /// Id клиента
        /// </summary>
        // ReSharper disable once UnusedMember.Global Ичпользуется
        public Guid ClientId { get; set; }
    }
}
