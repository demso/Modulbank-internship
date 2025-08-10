using FluentValidation;

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;

/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
// ReSharper disable once UnusedMember.Global
public class PerformTransactionValidator : AbstractValidator<PerformTransactionCommand>
{
    /// <summary>
    /// Создание валидатора и задание правил валидации
    /// </summary>
    public PerformTransactionValidator()
    {
        RuleFor(command => command.OwnerId).NotEmpty();
        RuleFor(command => command.AccountId).GreaterThan(0);
        RuleFor(command => command.Description).MaximumLength(255);
        RuleFor(command => command.Amount).GreaterThan(0);
    }
}