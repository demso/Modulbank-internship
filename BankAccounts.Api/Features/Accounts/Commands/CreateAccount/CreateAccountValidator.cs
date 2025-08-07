using FluentValidation;

namespace BankAccounts.Api.Features.Accounts.Commands;


/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
public class CreateCommandValidator : AbstractValidator<CreateAccountCommand>
{
    /// <summary>
    /// Создание валидатора и настройка правил
    /// </summary>
    public CreateCommandValidator()
    {
        RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
        RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
    }
}
