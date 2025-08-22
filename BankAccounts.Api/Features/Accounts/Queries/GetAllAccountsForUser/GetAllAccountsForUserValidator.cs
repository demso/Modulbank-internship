using FluentValidation;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAllAccountsForUser;

/// <summary>
/// Проверяющий запроса <see cref="GetAllAccountsForUserQuery"/>
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
// ReSharper disable once UnusedMember.Global
public class GetAllAccountsForUserValidator : AbstractValidator<GetAllAccountsForUserQuery>
{
    /// <inheritdoc />
    public GetAllAccountsForUserValidator()
    {
        RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
    }
}