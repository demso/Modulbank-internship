using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands.CreateAccount;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;

/// <summary>
/// Событие, генерируемое при открытии счета <seealso cref="CreateAccountHandler"/>
/// </summary>
public class AccountOpened : Event
{
    /// <summary>
    /// Id счета
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
    public int? AccountId { get; init; }
    /// <summary>
    /// Id владельца
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
    public Guid? OwnerId { get; init; }
    /// <summary>
    /// Валюта
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
    public Currencies? Currency { get; init; }
    /// <summary>
    /// Тип счета
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global Используется
    public AccountType? AccountType { get; init; }
}
