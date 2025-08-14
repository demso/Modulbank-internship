using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAccount;

/// <summary>
/// Обработчик запроса <see cref="GetAccountQuery"/>
/// </summary>
public class GetAccountHandler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : RequestHandlerBase<GetAccountQuery, AccountDto>
{
    /// <inheritdoc />
    public override async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        Account account = await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);

        return mapper.Map<AccountDto>(account);
    }
}