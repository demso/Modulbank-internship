namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity
{
    public class OutboxPublishedEntity  
    {
        public Guid Id { get; init; }
        public string Message { get; init; }
        public EventType EventType { get; init; }
        public required DateTime Created { get; init; } 
    }
}
