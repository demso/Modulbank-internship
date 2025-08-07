using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands.UpdateAccount;


/// <summary>
/// Команда для закрытия счета
/// </summary>
/// <param name="OwnerId">Id владельца</param>
/// <param name="AccountId">Id счета</param>
/// <param name="InterestRate">Процентная ставка</param>
/// <param name="Close">Нужно ли закрыть счет</param>
public record UpdateAccountCommand(
    Guid OwnerId,
    int AccountId,
    decimal? InterestRate,
    bool? Close
) : IRequest<Unit>;
