using AutoMapper;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;
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
    public class Handler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, IMapper mapper) : BaseRequestHandler<Query, List<TransactionDto>>
    {
        /// <inheritdoc />
        public override async Task<List<TransactionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
           await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);
            //var accountDtos = _mapper.Map<List<AccountDto>>(accounts);
            var entities = await transactionsRepository.GetByFilterAsync(request.AccountId, request.FromDate,
                request.ToDate, cancellationToken);

            var entitiesDto = mapper.Map<List<TransactionDto>>(entities);

            return entitiesDto;
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