using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
// ReSharper disable once UnusedType.Global Класс используется посредником

namespace BankAccounts.Api.Features.Transactions.Queries;

/// <summary>
///Получить все операции со счетом (выписка) 
/// </summary>
public static class GetTransactionsForAccount
{
    /// <summary>
    /// Запрос выписки
    /// </summary>
    /// <param name="OwnerId">Id владельца</param>
    /// <param name="AccountId">Id счета</param>
    /// <param name="FromDate">Начало периода (может быть null)</param>
    /// <param name="ToDate">Конец периода (может быть null)</param>
    public record Query(
        Guid OwnerId,
        int AccountId,
        DateOnly? FromDate,
        DateOnly? ToDate
    ) : IRequest<List<TransactionDto>>;
    /// <summary>
    /// Обработчик команды
    /// </summary>>
    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Query, List<TransactionDto>>
    {
        /// <inheritdoc />
        public override async Task<List<TransactionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
           await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            var entities = await dbDbContext.Transactions
                .Where(transaction => transaction.AccountId == request.AccountId
                    && (request.FromDate == null || DateOnly.FromDateTime(transaction.DateTime) >= request.FromDate.Value) 
                    && (request.ToDate == null || DateOnly.FromDateTime(transaction.DateTime) < request.ToDate.Value))
                .ProjectTo<TransactionDto>(mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities;
        }
    }

    /// <summary>
    /// Валидатор команды
    /// </summary>
    public class QueryValidator : AbstractValidator<Query>
    {
        /// <summary>
        /// Создание валидатора и настройка правил
        /// </summary>
        public QueryValidator()
        {
            RuleFor(query => query.AccountId).GreaterThan(0);
            RuleFor(query => query.FromDate)
                .GreaterThan(new DateOnly(1900, 1, 1))
                .When(query => query.FromDate is not null)
                .DependentRules(() =>
                    RuleFor(query => query.ToDate)
                        .GreaterThan(query => query.FromDate)
                        .When(query => query.ToDate is not null)
                        .WithMessage("Конец периода должен быть позже начала периода."))
                .When(query => query.FromDate is not null);
           
        }
    }
}