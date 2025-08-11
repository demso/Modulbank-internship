using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;

namespace BankAccounts.Api.Features.Accounts;

/// <summary>
/// Класс представляющий собой счет в банковском сервисе
/// </summary>
public class Account
{
    /// <summary>
    /// Уникальный идентификатор счета
    /// </summary>
    public int AccountId { get; init; }
    /// <summary>
    /// Уникальный идентификатор владельца
    /// </summary>
    public Guid OwnerId { get; init; }
    /// <summary>
    /// Тип счета
    /// </summary>
    public AccountType AccountType { get; init; } = AccountType.Checking;
    /// <summary>
    /// Валюта счета
    /// </summary>
    public Currencies Currency { get; init; } = Currencies.Rub;
    /// <summary>
    /// Баланс счета
    /// </summary>
    public decimal Balance { get; set; }
    /// <summary>
    /// Процентная ставка по счету
    /// </summary>
    public decimal InterestRate { get; set; }
    /// <summary>
    /// Дата открытия счета
    /// </summary>
    public DateTime OpenDate { get; init; }
    /// <summary>
    /// Дата закрытия счета
    /// </summary>
    public DateTime? CloseDate { get; set; }
    /// <summary>
    /// Список транзакций по счету
    /// </summary>
    public List<Transaction> Transactions { get; init; } = [];
    // ReSharper disable once CommentTypo Наименование верно
    /// <summary>
    /// Concurrency‑token (xmin) для оптимистичной блокировки
    /// </summary>
    public uint Version { get; init; }
}