using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Transactions.Dtos;

public record PerformTransferDto(
    [Required] int? FromAccountId,
    [Required] int? ToAccountId,
    [Required] decimal? Amount
);