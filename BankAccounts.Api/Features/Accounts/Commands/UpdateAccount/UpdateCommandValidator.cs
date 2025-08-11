using FluentValidation;

namespace BankAccounts.Api.Features.Accounts.Commands.UpdateAccount;

/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
// ReSharper disable once UnusedMember.Global 
public class UpdateCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    /// <summary>
    /// Создание валидатора и настройка правил
    /// </summary>
    public UpdateCommandValidator()
    {
        RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
        RuleFor(command => command.AccountId).GreaterThan(0);
        RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
    }
}