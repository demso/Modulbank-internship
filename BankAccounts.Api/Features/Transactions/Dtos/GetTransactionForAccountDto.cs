using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Transactions.Dtos;

public record GetTransactionForAccountDto(
    DateOnly? FromDate,
    DateOnly? ToDate
);