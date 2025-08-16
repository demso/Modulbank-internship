using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity
{
    public class InboxConsumedEntity
    {
        public Guid MessageId { get; init; }
        public EventType EventType { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Handler { get; set; }
    };
}
