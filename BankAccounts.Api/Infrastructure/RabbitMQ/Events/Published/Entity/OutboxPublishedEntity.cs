using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity
{
    public class OutboxPublishedEntity  
    {
        public Guid Id { get; init; }
        public string Message { get; set; }
        public EventType EventType { get; init; }
        public required DateTime Created { get; init; } 
        public uint TryCount { get; set; }
    }
}
