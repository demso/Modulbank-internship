using BankAccounts.Api.Features.Accounts.Dtos;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAccount;

/// <summary>
/// Запрос получения данных о счете
/// </summary>
/// <param name="OwnerId">Id владельца</param>
/// <param name="AccountId">Id счета</param>
public record GetAccountQuery(
    Guid OwnerId,
    int AccountId
) : IRequest<AccountDto>;