using BankAccounts.Api.Features.Shared;
using System.ComponentModel.DataAnnotations;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record CreateAccountDto(
    [Required] AccountType? AccountType,
    [Required] CurrencyService.Currencies? Currency,
     decimal? InterestRate
);