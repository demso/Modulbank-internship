using System.ComponentModel.DataAnnotations;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace BankAccounts.Api.Features.Transactions.Dtos;

/// <summary>
/// DTO для передачи данных о требуемом трансфере
/// </summary>
/// <param name="FromAccountId">Исходный счет</param>
/// <param name="ToAccountId">Счет назначения</param>
/// <param name="Amount">Сумма денежных средств</param>
public record PerformTransferDto(
    [Required] int? FromAccountId,
    [Required] int? ToAccountId,
    [Required] decimal? Amount
);