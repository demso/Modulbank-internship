using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Transactions.Dtos;

public record GetTransactionForAccountDto(
    DateTime? FromDate,
    DateTime? ToDate
);