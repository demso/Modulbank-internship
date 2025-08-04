using System.ComponentModel.DataAnnotations;
using BankAccounts.Api.Infrastructure.CurrencyService;

// ReSharper disable UnusedAutoPropertyAccessor.Global Свойства используются

namespace BankAccounts.Api.Features.Transactions;

/// <summary>
/// Класс, представляющий транзакцию
/// </summary>
public class Transaction
{
    /// <summary>
    /// Id транзакции
    /// </summary>
    public Guid TransactionId { get; init; }
    /// <summary>
    /// Id счета
    /// </summary>
    public int AccountId { get; init; }
    /// <summary>
    /// Сумма денежных средств
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// Валюта
    /// </summary>
    public Currencies Currency { get; init; }
    /// <summary>
    /// Тип транзакции
    /// </summary>
    public TransactionType TransactionType { get; init; }
    /// <summary>
    /// Описание транзакции
    /// </summary>
    [MaxLength(255)]
    public string? Description { get; init; }
    /// <summary>
    /// Дата и время проведения транзакции
    /// </summary>
    public DateTime DateTime { get; init; }
}