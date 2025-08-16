using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

public abstract class Event
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required Metadata Metadata { get; init; }
    
    private static Dictionary<EventType, string> _eventMap = new Dictionary<EventType, string>()
    {
        {EventType.AccountOpened, "account.opened"},
        {EventType.InterestAccrued, "money.interest.accrued"},
        {EventType.MoneyCredited, "money.credited"},
        {EventType.MoneyDebited, "money.debited"},
        {EventType.TransferCompleted, "transfer.completed"},
        {EventType.ClientBlocked, "client.blocked"},
        {EventType.ClientUnblocked, "client.unblocked"},
    };
        
    public static string GetRoute(EventType type)
    {
        return _eventMap[type];
    }

    public static EventType GetEventType(Event type)
    {
        return type switch
        {
            AccountOpened => EventType.AccountOpened,
            InterestAccrued => EventType.InterestAccrued,
            MoneyCredited => EventType.MoneyCredited,
            MoneyDebited => EventType.MoneyDebited,
            TransferCompleted => EventType.TransferCompleted,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
