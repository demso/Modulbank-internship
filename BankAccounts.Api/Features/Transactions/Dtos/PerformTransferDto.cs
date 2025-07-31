namespace BankAccounts.Api.Features.Transactions.Dtos;

public record PerformTransferDto(
    int FromAccountId,
    int ToAccountId,
    decimal Amount
);