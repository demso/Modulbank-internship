using FluentValidation;

namespace BankAccounts.Api.Features.Transactions.Queries.GetTransaction;

/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
public class GetTransactionValidator : AbstractValidator<GetTransactionQuery>
{
    /// <summary>
    /// Создание валидатора и настройка правил
    /// </summary>
    public GetTransactionValidator()
    {
        RuleFor(command => command.OwnerId).NotEmpty();
        RuleFor(command => command.TransactionId).NotEmpty();
    }
}