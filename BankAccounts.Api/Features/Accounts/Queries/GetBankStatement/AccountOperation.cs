namespace BankAccounts.Api.Features.Accounts.Queries.GetBankStatement;

/// <summary>
/// Операция по счету
/// </summary>
/// <param name="DateTime">Время проведения</param>
/// <param name="CounterPartyId">Счет получателя (если есть)</param>
/// <param name="Sum">Сумма</param>
/// <param name="AfterBalance">Баланс после операции</param>
/// <param name="Description">Описание</param>
// ReSharper disable NotAccessedPositionalProperty.Global Свойства используются
public record AccountOperation(
    DateTime DateTime,
    int CounterPartyId,
    decimal Sum,
    decimal AfterBalance,
    string? Description
);