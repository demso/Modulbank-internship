namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter
{
    public class DeadLetterEntity //inbox_dead_letters (message_id, received_at, handler, payload, error).
    {
        public Guid MessageId { get; set; }
        public DateTime RecievedAt { get; set; }
        public string Handler { get; set; }
        public string Payload { get; set; }
        public EventType? EventType { get; set; }
        public string Error { get; set; }
    }
}
