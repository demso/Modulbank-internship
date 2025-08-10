using FluentValidation;

namespace BankAccounts.Api.Features.Accounts.Commands.CreateAccount;


/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    /// <summary>
    /// Создание валидатора и настройка правил
    /// </summary>
    public CreateAccountCommandValidator()
    {
        RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
        RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
    }
}
