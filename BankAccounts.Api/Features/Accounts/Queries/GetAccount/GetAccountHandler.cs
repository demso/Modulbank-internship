using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;

namespace BankAccounts.Api.Features.Accounts.Queries.GetAccount;

/// <summary>
/// Обработчик запроса
/// </summary>
public class GetAccountHandler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : BaseRequestHandler<GetAccountQuery, AccountDto>
{
    /// <inheritdoc />
    public override async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);

        return mapper.Map<AccountDto>(account);
    }
}