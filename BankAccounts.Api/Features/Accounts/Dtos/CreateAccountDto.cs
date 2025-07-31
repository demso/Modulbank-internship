using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record CreateAccountDto(
    [Required] Guid? OwnerId,
    [Required] AccountType? AccountType,
    [Required] CurrencyService.Currencies? Currency,
     decimal? InterestRate
);