using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity
{
    /// <summary>
    /// Сущность для хранения информации об обработанных сообщениях из RabbitMQ.
    /// Используется для предотвращения дублирования обработки сообщений (inbox pattern).
    /// </summary>
    public class InboxConsumedEntity
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Уникальный идентификатор сообщения RabbitMQ.
        /// </summary>
        public Guid MessageId { get; init; }

        /// <summary>
        /// Тип события, связанного с сообщением.
        /// </summary>
        public EventType EventType { get; init; }

        /// <summary>
        /// Дата и время обработки сообщения.
        /// </summary>
        public DateTime ProcessedAt { get; init; }

        /// <summary>
        /// Имя обработчика, который обработал сообщение.
        /// </summary>
        public required string Handler { get; init; }
    }
}