namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter
{
    /// <summary>
    /// Сущность таблицы inbox_dead_letters, представляющая собой неправильное сообщение
    /// </summary>
    public class DeadLetterEntity
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Id сообщения
        /// </summary>
        public Guid MessageId { get; init; }
        /// <summary>
        /// Время/дата приемки сообщения
        /// </summary>
        public DateTime ReceivedAt { get; init; }
        /// <summary>
        /// Название обработчика сообщения
        /// </summary>
        public required string Handler { get; init; }
        /// <summary>
        /// Тело сообщения
        /// </summary>
        public required string Payload { get; init; }
        /// <summary>
        /// Тип события
        /// </summary>
        public EventType? EventType { get; init; }
        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public required string Error { get; init; }
    }
}
