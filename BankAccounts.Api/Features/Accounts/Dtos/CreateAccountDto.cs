using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record CreateAccountDto(
    [Required] AccountType? AccountType,
    [Required] CurrencyService.Currencies? Currency,
     decimal? InterestRate
);