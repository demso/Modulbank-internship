using BankAccounts.Api.Features.Shared;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BankAccounts.Api.Features.Transactions;

public class Transaction
{
    public Guid TransactionId { get; init; }
    public int AccountId { get; init; }
    public decimal Amount { get; init; }
    public CurrencyService.Currencies Currency { get; init; }
    public TransactionType TransactionType { get; init; }
    [MaxLength(255)]
    public string? Description { get; init; }
    public DateTime DateTime { get; init; }
}