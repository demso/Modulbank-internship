using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity
{
    public class OutboxPublishedEntity  
    {
        public Guid Id { get; init; }
        public required string Message { get; set; }
        public  Guid EventId { get; set; }
        public required Guid CorrelationId { get; set; }
        public required Guid CausationId { get; set; }
        public EventType EventType { get; init; }
        public required DateTime Created { get; init; } 
        public uint TryCount { get; set; }
    }
}
