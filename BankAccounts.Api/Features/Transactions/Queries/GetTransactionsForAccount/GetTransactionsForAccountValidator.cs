﻿using FluentValidation;

namespace BankAccounts.Api.Features.Transactions.Queries.GetTransactionsForAccount;

/// <summary>
/// Валидатор команды
/// </summary>
// ReSharper disable once UnusedType.Global Класс используется посредником
// ReSharper disable once UnusedMember.Global
public class GetTransactionsForAccountValidator : AbstractValidator<GetTransactionsForAccountQuery>
{
    /// <summary>
    /// Создание валидатора и настройка правил
    /// </summary>
    public GetTransactionsForAccountValidator()
    {
        RuleFor(query => query.AccountId).GreaterThan(0);
        RuleFor(query => query.FromDate)
            .GreaterThan(new DateOnly(1900, 1, 1))
            .When(query => query.FromDate is not null)
            .DependentRules(() =>
                RuleFor(query => query.ToDate)
                    .GreaterThanOrEqualTo(query => query.FromDate)
                    .When(query => query.ToDate is not null)
                    .WithMessage("Конец периода должен быть позже начала периода."))
            .When(query => query.FromDate is not null);
           
    }
}