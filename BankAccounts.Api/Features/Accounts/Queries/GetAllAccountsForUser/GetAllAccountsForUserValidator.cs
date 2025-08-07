using FluentValidation;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAllAccountsForUser;

/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
public class GetAllAccountsForUserValidator : AbstractValidator<GetAllCountsForUserQuery>
{
    /// <summary>
    /// Создание валидатора и настройка правил
    /// </summary>
    public GetAllAccountsForUserValidator()
    {
        RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
    }
}