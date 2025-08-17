using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity
{
    /// <summary>
    /// Сущность таблицы outbox_published
    /// </summary>
    public class OutboxPublishedEntity  
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Событие для отправки в json формате
        /// </summary>
        public required string Message { get; init; }
        /// <summary>
        /// Id события
        /// </summary>
        public  Guid EventId { get; init; }
        /// <summary>
        /// Id для определения принадлежности одной операции
        /// </summary>
        public required Guid CorrelationId { get; init; }
        /// <summary>
        /// Id причины события
        /// </summary>
        public required Guid CausationId { get; init; }
        /// <summary>
        /// Тип события
        /// </summary>
        public EventType EventType { get; init; }
        /// <summary>
        /// Время создания
        /// </summary>
        public required DateTime Created { get; init; } 
        /// <summary>
        /// Количество попыток на отправку
        /// </summary>
        public uint TryCount { get; set; }
    }
}
