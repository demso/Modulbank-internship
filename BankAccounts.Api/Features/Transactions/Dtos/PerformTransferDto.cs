namespace BankAccounts.Api.Features.Transactions.Dtos;

public record PerformTransferDto(
    Guid OwnerId,
    int FromAccountId,
    int ToAccountId,
    decimal Amount
);