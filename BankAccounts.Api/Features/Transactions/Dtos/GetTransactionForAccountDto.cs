using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Transactions.Dtos;

public record GetTransactionForAccountDto(
    [Required] int? AccountId,
    DateTime? FromDate,
    DateTime? ToDate
);