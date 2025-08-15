namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared
{
    public class Metadata
    {
        public required Guid CausationId { get; set; }
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public string Version { get; set; } = "v1";
        public string Source { get; set; } = "account-service";
    }
}
