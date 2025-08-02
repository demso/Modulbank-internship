using System.ComponentModel.DataAnnotations;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace BankAccounts.Api.Features.Transactions.Dtos;

public record PerformTransferDto(
    [Required] int? FromAccountId,
    [Required] int? ToAccountId,
    [Required] decimal? Amount
);