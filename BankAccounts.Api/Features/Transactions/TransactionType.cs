namespace BankAccounts.Api.Features.Transactions;

/// <summary>
/// Перечисление с типами транзакции
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Пополнение счета
    /// </summary>
    Debit,
    /// <summary>
    /// Снятие средств со счета
    /// </summary>
    Credit
}