using FluentValidation;

namespace BankAccounts.Api.Features.Accounts.Commands.CreateAccount;


/// <summary>
/// Проверяющий команды <see cref="CreateAccountCommand"/>
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    /// <summary>
    /// Создание проверяющего и настройка правил
    /// </summary>
    public CreateAccountValidator()
    {
        RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
        RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
    }
}
