using System.ComponentModel.DataAnnotations;
// ReSharper disable NotAccessedPositionalProperty.Global Свойства используются

namespace BankAccounts.Api.Features.Transactions.Dtos;

/// <summary>
/// DTO для передачи проведения транзакции
/// </summary>
/// <param name="AccountId">Id счета</param>
/// <param name="TransactionType">Тип транзакции</param>
/// <param name="Amount">Сумма денежных средств</param>
/// <param name="Description">Описание</param>
public record PerformTransactionDto(
    [Required] int? AccountId,
    [Required] TransactionType? TransactionType,
    [Required] decimal? Amount,
    string? Description
);