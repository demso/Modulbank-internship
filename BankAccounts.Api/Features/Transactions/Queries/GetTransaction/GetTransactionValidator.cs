using FluentValidation;

namespace BankAccounts.Api.Features.Transactions.Queries.GetTransaction;

/// <summary>
/// Валидатор запроса <see cref="GetTransactionQuery"/>
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
// ReSharper disable once UnusedMember.Global
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