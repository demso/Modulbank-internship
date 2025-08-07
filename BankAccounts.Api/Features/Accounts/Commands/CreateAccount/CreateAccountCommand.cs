using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure.CurrencyService;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

/// <summary>
/// Команда для создания счета
/// </summary>
public record CreateAccountCommand : IRequest<AccountDto>
{
    /// <summary>
    /// Id пользователя
    /// </summary>
    public Guid OwnerId { get; set; }
    /// <summary>
    /// Тип счета
    /// </summary>
    public AccountType AccountType { get; init; }
    /// <summary>
    /// Валюта
    /// </summary>
    public Currencies Currency { get; init; }
    /// <summary>
    /// Процентная ставка
    /// </summary>
    public decimal InterestRate { get; init; }
}
