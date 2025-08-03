using BankAccounts.Api.Features.Shared;

namespace BankAccounts.Api.Features.Transactions.Dtos;

public record TransactionDto(
    Guid TransactionId,
    int AccountId,
    decimal Amount,
    CurrencyService.Currencies Currency,
    TransactionType? TransactionType,
    string? Description,
    DateTime DateTime
    );