using MediatR;

namespace BankAccounts.Api.Features.Accounts.Queries.GetBankStatement;

/// <summary>
/// Запрос выписки
/// </summary>
/// <param name="OwnerId">Id владельца</param>
/// <param name="Username">Имя пользователя</param>
/// <param name="AccountId">Id счета</param>
/// <param name="FromDate">Начало периода (может быть null)</param>
/// <param name="ToDate">Конец периода (может быть null)</param>
// ReSharper disable once ClassNeverInstantiated.Global Класс используется в качестве запроса для MediatR
public record GetBankStatementQuery(
    Guid OwnerId,
    string Username,
    int AccountId,
    DateOnly? FromDate,
    DateOnly? ToDate
) : IRequest<BankStatement>;