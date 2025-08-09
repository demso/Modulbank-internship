using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAllAccountsForUser;

/// <summary>
/// Обработчик запроса
/// </summary>
public class GetAllAccountsForUserHandler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : BaseRequestHandler<GetAllCountsForUserQuery, List<AccountDto>>
{
    /// <inheritdoc />
    public override async Task<List<AccountDto>> Handle(GetAllCountsForUserQuery request, CancellationToken cancellationToken)
    {
        var entities = await accountsRepository.GetByFilterAsync(request.OwnerId, cancellationToken);
        // сортируем счета для удобного отображения
        entities.Sort((a1, a2) =>
            a1.AccountId > a2.AccountId ? 1 : a1.AccountId == a2.AccountId ? 0 : -1);

        var entitiesDto = mapper.Map<List<AccountDto>>(entities);

        return entitiesDto;
    }
}