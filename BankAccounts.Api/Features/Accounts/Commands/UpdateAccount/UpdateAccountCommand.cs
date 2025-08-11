using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands.UpdateAccount;


/// <summary>
/// ������� ��� �������� �����
/// </summary>
/// <param name="OwnerId">Id ���������</param>
/// <param name="AccountId">Id �����</param>
/// <param name="InterestRate">���������� ������</param>
/// <param name="Close">����� �� ������� ����</param>
public record UpdateAccountCommand(
    Guid OwnerId,
    int AccountId,
    decimal? InterestRate,
    bool? Close
) : IRequest<Unit>;
