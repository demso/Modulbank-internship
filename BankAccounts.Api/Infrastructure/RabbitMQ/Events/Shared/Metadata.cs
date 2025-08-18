namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared
{
    /// <summary>
    /// Мета информация для события <see cref="Event"/>
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Id причины события
        /// </summary>
        public required Guid CausationId { get; init; }
        /// <summary>
        /// Id для сопоставления сообщения
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global Метод set нужен
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        /// <summary>
        /// Версия сообщения для совместимости
        /// </summary>
        // ReSharper disable once UnusedMember.Global Используется
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        public string Version { get; set; } = "v1";
        /// <summary>
        /// Сервис-источник сообщений
        /// </summary>
        // ReSharper disable once UnusedMember.Global Используется
        // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
        public string Source { get; set; } = "account-service";
    }
}
