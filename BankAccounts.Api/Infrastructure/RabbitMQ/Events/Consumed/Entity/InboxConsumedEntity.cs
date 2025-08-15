namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity
{
    public record InboxConsumedEntity(Guid Id, string Message, EventType EventType);
}
