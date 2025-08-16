using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events
{
    public class TransferCompleted : Event
    {
        public int? SourceAccountId { get; set; }
        public int? DestinationAccountId { get; set; }
        public decimal? Amount { get; set; }
        public Currencies? Currency { get; set; }
        public Guid? TransferId { get; set; }
    }
}
