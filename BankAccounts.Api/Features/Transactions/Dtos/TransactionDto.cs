using BankAccounts.Api.Features.CurrencyService;

namespace BankAccounts.Api.Features.Transactions.Dtos;

/// <summary>
/// DTO для передачи данных о транзакции
/// </summary>
/// <param name="TransactionId">Id транзакции</param>
/// <param name="AccountId">Id счета</param>
/// <param name="Amount">Сумма денежных средств</param>
/// <param name="Currency">Валюта</param>
/// <param name="TransactionType">Тип транзакции</param>
/// <param name="Description">Описание</param>
/// <param name="DateTime">Дата и время проведения транзакции</param>
public record TransactionDto(
    Guid TransactionId,
    int AccountId,
    decimal Amount,
    Currencies Currency,
    TransactionType? TransactionType,
    string? Description,
    DateTime DateTime
    );