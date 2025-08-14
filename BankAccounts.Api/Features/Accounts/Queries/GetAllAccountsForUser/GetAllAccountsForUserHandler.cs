using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAllAccountsForUser;

/// <summary>
/// Обработчик запроса <see cref="GetAllAccountsForUserQuery"/>
/// </summary>
public class GetAllAccountsForUserHandler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : RequestHandlerBase<GetAllAccountsForUserQuery, List<AccountDto>>
{
    /// <inheritdoc />
    public override async Task<List<AccountDto>> Handle(GetAllAccountsForUserQuery request, CancellationToken cancellationToken)
    {
        List<Account> entities = await accountsRepository.GetByFilterAsync(request.OwnerId, cancellationToken);
        // сортируем счета для удобного отображения от меньших по Id до больших
        entities.Sort((a1, a2) =>
            a1.AccountId > a2.AccountId ? 1 : a1.AccountId == a2.AccountId ? 0 : -1);

        List<AccountDto>? entitiesDto = mapper.Map<List<AccountDto>>(entities);

        return entitiesDto;
    }
}