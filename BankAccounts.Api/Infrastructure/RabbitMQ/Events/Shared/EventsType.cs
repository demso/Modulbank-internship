namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events
{
    public enum EventType
    {
        AccountOpened,
        InterestAccrued,
        MoneyCredited,
        MoneyDebited,
        TransferComplited,
        ClientBlocked,
        ClientUnblocked
    }

//     public static class Event
//     {
//         private static Dictionary<EventType, string> eventMap = new Dictionary<EventType, string>()
//         {
//             {EventType.AccountOpened, "account.opened"},
//             {EventType.InterestAccrued, "money.interest.accrued"},
//             {EventType.MoneyCredited, "money.credited"},
//             {EventType.MoneyDebited, "money.debited"},
//             {EventType.TransferComplited, "transfer.completed"},
//         };
//         
//         public static string GetRoute(EventType type)
//         {
//             return eventMap[type];
//         }
//     }
 }