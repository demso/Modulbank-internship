using BankAccounts.Api.Infrastructure.CurrencyService;

namespace BankAccounts.Api.Features.Accounts.Dtos;

/// <summary>
/// DTO для передачи данных о счете
/// </summary>
/// <param name="AccountId">Id счета</param>
/// <param name="AccountType">Тип аккаунта</param>
/// <param name="Currency">Валюта счета</param>
/// <param name="Balance">Баланс</param>
/// <param name="InterestRate">Процентная ставка</param>
/// <param name="OpenDate">Дата и время открытия</param>
/// <param name="CloseDate">Дата и время закрытия</param>
public record AccountDto(
    int AccountId,
    AccountType AccountType,
    Currencies Currency,
    decimal Balance,
    decimal? InterestRate,
    DateTime OpenDate,
    DateTime? CloseDate
);