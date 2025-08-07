using FluentValidation;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAccount;

/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
public class GetAccountValidator : AbstractValidator<GetAccountQuery>
{
    /// <summary>
    /// Создание валидатора и настройка правил
    /// </summary>
    public GetAccountValidator()
    {
        RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
        RuleFor(command => command.AccountId).GreaterThan(0);
    }
}