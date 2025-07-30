using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Accounts;

public record AccountDto(
    Guid AccountId,
    Guid OwnerId,
    AccountType AccountType,
    string Currency,
    decimal Balance,
    decimal? InterestRate,
    DateTime OpenDate,
    DateTime? CloseDate
);