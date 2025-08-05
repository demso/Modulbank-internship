namespace BankAccounts.Api.Features.Accounts;

/// <summary>
/// Тип аккаунта
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Текущий счет
    /// </summary>
    Checking,
    /// <summary>
    /// Депозитный счет
    /// </summary>
    Deposit,
    /// <summary>
    /// Крелитный счет
    /// </summary>
    // ReSharper disable once UnusedMember.Global Тип используется
    Credit
}