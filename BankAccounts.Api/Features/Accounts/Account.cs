using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Accounts;

//TODO validation of these fields

public class Account
{
    public Guid AccountId { get; init; }
    public Guid OwnerId { get; set; }
    public AccountType AccountType { get; set; }
    public required string Currency { get; set; }
    public decimal Balance { get; set; }
    public decimal? InterestRate { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime CloseDate { get; set; }
}