using System.ComponentModel.DataAnnotations;
using BankAccounts.Api.Infrastructure.CurrencyService;

// ReSharper disable NotAccessedPositionalProperty.Global Параметры используются

namespace BankAccounts.Api.Features.Accounts.Dtos;

/// <summary>
/// Запись для передачи данных о создаваемом счете
/// </summary>
/// <param name="AccountType">Тип счета</param>
/// <param name="Currency">Валюта</param>
/// <param name="InterestRate">Прочентная ставка</param>
public record CreateAccountDto(
    [Required] AccountType? AccountType,
    [Required] Currencies? Currency,
     decimal? InterestRate
);