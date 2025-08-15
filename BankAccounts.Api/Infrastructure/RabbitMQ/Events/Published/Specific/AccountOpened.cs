using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;

public class AccountOpened : Event
{
    public int? AccountId { get; init; }
    public Guid? OwnerId { get; init; }
    public Currencies? Currency { get; init; }
    public AccountType? AccountType { get; init; }
}
