using BankAccounts.Api.Infrastructure.CurrencyService;

namespace BankAccounts.Api.Features.Accounts.Queries.GetBankStatement;

// ReSharper disable file NotAccessedPositionalProperty.Global Свойства используются

/// <summary>
/// ВЫписка о банковских операция по счету
/// </summary>
/// <param name="AccountId">Id аккаунта</param>
/// <param name="Username">Имя пользователя</param>
/// <param name="Currency">Валюта</param>
/// <param name="CreationDateTime">Время создания выписки</param>
/// <param name="Operations">Операции</param>
/// <param name="StartBalance">Баланс на начало периода</param>
/// <param name="EndBalance">Баланс на конец периода</param>
public record BankStatement(
    int AccountId,
    string Username,
    Currencies Currency,
    DateTime CreationDateTime,
    List<AccountOperation> Operations,
    decimal StartBalance,
    decimal EndBalance,
    DateOnly StartPeriod,
    DateOnly EndPeriod
);