namespace BankAccounts.Api.Features.Transactions.Dtos;

public record GetAllTransactionForAccountDto(
    Guid UserId,
    int AccountId
);