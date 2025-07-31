using BankAccounts.Api.Features.Accounts;

namespace BankAccounts.Api.Features.Transactions;

public class Transaction
{
    public Guid TransactionId { get; set; }
    public int AccountId { get; set; }
    public Guid? CounterpartyAccountId { get; set; }
    public decimal Amount { get; set; }
    public CurrencyService.Currencies Currency { get; set; }
    public TransactionType TransactionType { get; set; }
    public string? Description { get; set; }
    public DateTime DateTime { get; set; }
}