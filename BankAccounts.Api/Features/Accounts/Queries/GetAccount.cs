using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Database;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Queries;

/// <summary>
/// Получение данных о счете
/// </summary>
public static class GetAccount
{
    /// <summary>
    /// Запрос получения данных о счете
    /// </summary>
    /// <param name="OwnerId">Id владельца</param>
    /// <param name="AccountId">Id счета</param>
    public record Query(
        Guid OwnerId,
        int AccountId
    ) : IRequest<AccountDto>;
    
    /// <summary>
    /// Обработчик запроса
    /// </summary>
    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Query, AccountDto>
    {
        /// <inheritdoc />
        public override async Task<AccountDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            return mapper.Map<AccountDto>(account);
        }
    }

    /// <summary>
    /// Валидатор команды
    /// </summary>
    // ReSharper disable once UnusedType.Global Класс используется посредником
    public class QueryValidator : AbstractValidator<Query>
    {
        /// <summary>
        /// Создание валидатора и настройка правил
        /// </summary>
        public QueryValidator()
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
            RuleFor(command => command.AccountId).GreaterThan(0);
        }
    }
}

