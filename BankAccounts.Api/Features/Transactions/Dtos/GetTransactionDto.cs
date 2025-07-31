namespace BankAccounts.Api.Features.Transactions.Dtos;

public record GetTransactionDto(Guid TransactionId, Guid UserId);