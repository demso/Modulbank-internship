namespace BankAccounts.Api.Features.Transactions.Dtos;

public record PerformTransactionDto(
    int AccountId,
    TransactionType TransactionType,
    decimal Amount
);