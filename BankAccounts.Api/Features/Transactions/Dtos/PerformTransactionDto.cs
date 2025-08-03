using System.ComponentModel.DataAnnotations;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace BankAccounts.Api.Features.Transactions.Dtos;

public record PerformTransactionDto(
    [Required] int? AccountId,
    [Required] TransactionType? TransactionType,
    [Required] decimal? Amount,
    string? Description
);