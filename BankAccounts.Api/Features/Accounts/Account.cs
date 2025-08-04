using BankAccounts.Api.Features.Shared;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace BankAccounts.Api.Features.Accounts;

public class Account
{
    public int AccountId { get; init; }
    public Guid OwnerId { get; init; }
    public AccountType AccountType { get; init; }
    public CurrencyService.Currencies Currency { get; init; }
    public decimal Balance { get; set; }
    public decimal? InterestRate { get; set; }
    public DateTime OpenDate { get; init; }
    public DateTime? CloseDate { get; set; }
}