using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Accounts.Queries;

/// <summary>
/// Получить все счета пользователя
/// </summary>
public static class GetAllAccountsForUser
{
    /// <summary>
    /// Запрос всех счетов пользователя
    /// </summary>
    /// <param name="OwnerId">Id владельца счетов</param>
    public record Query(Guid OwnerId) : IRequest<List<AccountDto>>;

    /// <summary>
    /// Обработчик запроса
    /// </summary>
    public class Handler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : BaseRequestHandler<Query, List<AccountDto>>
    {
        /// <inheritdoc />
        public override async Task<List<AccountDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entities = await accountsRepository.GetByFilterAsync(request.OwnerId, cancellationToken);
            
            var entitiesDto = mapper.Map<List<AccountDto>>(entities);

            return entitiesDto;
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
        }
    }
}