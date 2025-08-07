using BankAccounts.Api.Features.Accounts.Dtos;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAllAccountsForUser;

/// <summary>
/// Запрос всех счетов пользователя
/// </summary>
/// <param name="OwnerId">Id владельца счетов</param>
public record GetAllCountsForUserQuery(Guid OwnerId) : IRequest<List<AccountDto>>;