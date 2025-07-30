namespace BankAccounts.Api.Features.Accounts.Dtos;

public record CreateAccountDto(
    Guid OwnerId,
    AccountType AccountType,
    string Currency,
    decimal? InterestRate
);