using FluentValidation;

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;

/// <summary>
/// Валидатор команды <see cref="PerformTransferCommand"/>
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
// ReSharper disable once UnusedMember.Global
public class PerformTransferValidator : AbstractValidator<PerformTransferCommand>
{
        
    /// <inheritdoc />
    public PerformTransferValidator()
    {
        RuleFor(command => command.OwnerId).NotEmpty();
        RuleFor(command => command.FromAccountId).GreaterThan(0).NotEqual(command => command.ToAccountId);
        RuleFor(command => command.ToAccountId).GreaterThan(0);
        RuleFor(command => command.Amount).GreaterThan(0);
    }
}