using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

/// <summary>
/// Абстрактный класс события, создаваемого при определенных условиях
/// </summary>
public abstract class Event
{
    /// <summary>
    /// Id события
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();
    /// <summary>
    /// Время/дата создания 
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    /// <summary>
    /// Мета-дата
    /// </summary>
    public required Metadata Meta { get; init; }
    
    private static readonly Dictionary<EventType, string> EventMap = new()
    {
        {EventType.AccountOpened, "account.opened"},
        {EventType.InterestAccrued, "money.interest.accrued"},
        {EventType.MoneyCredited, "money.credited"},
        {EventType.MoneyDebited, "money.debited"},
        {EventType.TransferCompleted, "transfer.completed"},
        {EventType.ClientBlocked, "client.blocked"},
        {EventType.ClientUnblocked, "client.unblocked"}
    };
        
    /// <summary>
    /// Вернет путь для публикации события
    /// </summary>
    /// <param name="type">Тип события</param>
    /// <returns>Путь</returns>
    public static string GetRoute(EventType type)
    {
        return EventMap[type];
    }

    /// <summary>
    /// Получить тип сообщения по типу объекта
    /// </summary>
    /// <param name="type">Объект события</param>
    /// <returns>Тип события</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
