namespace BankAccounts.Api.Features.Accounts.Dtos;

public record AccountDto(
    int AccountId,
    Guid OwnerId,
    AccountType AccountType,
    CurrencyService.Currencies Currency,
    decimal Balance,
    decimal? InterestRate,
    DateTime OpenDate,
    DateTime? CloseDate
);