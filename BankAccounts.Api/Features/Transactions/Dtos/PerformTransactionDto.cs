namespace BankAccounts.Api.Features.Transactions.Dtos;

public record PerformTransactionDto(
    Guid OwnerId,
    int AccountId,
    TransactionType TransactionType,
    decimal Amount
);