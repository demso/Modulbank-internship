using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Accounts;

//TODO validation of these fields

public class Account
{
    public int AccountId { get; init; }
    public Guid OwnerId { get; init; }
    public AccountType AccountType { get; init; }
    public CurrencyService.Currencies Currency { get; init; }
    public decimal Balance { get; set; }
    public decimal? InterestRate { get; set; }
    public DateTime OpenDate { get; init; }
    public DateTime? CloseDate { get; init; }
}