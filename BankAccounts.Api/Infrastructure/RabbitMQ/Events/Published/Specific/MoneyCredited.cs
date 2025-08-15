using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;

public class MoneyCredited : Event
{
    public int? AccountId { get; set; }
    public decimal? Amount { get; set; }
    public  Currencies? Currency { get; set; }
    public Guid? OperationId { get; set; }
}

