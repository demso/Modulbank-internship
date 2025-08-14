using AutoMapper;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;

// ReSharper disable once UnusedType.Global Класс используется посредником

namespace BankAccounts.Api.Features.Transactions.Queries.GetTransactionsForAccount;

/// <summary>
/// Обработчик команды запроса <see cref="GetTransactionsForAccountQuery"/>
/// </summary>>
public class GetTransactionForAccountHandler(IAccountsRepositoryAsync accountsRepository,
    ITransactionsRepositoryAsync transactionsRepository, IMapper mapper) 
    : RequestHandlerBase<GetTransactionsForAccountQuery, List<TransactionDto>>
{
    /// <inheritdoc />
    public override async Task<List<TransactionDto>> Handle(GetTransactionsForAccountQuery request,
        CancellationToken cancellationToken)
    {
       await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);
   
        List<Transaction> entities = await transactionsRepository.GetByFilterAsync(request.AccountId, request.FromDate,
            request.ToDate, cancellationToken);

        List<TransactionDto>? entitiesDto = mapper.Map<List<TransactionDto>>(entities);

        return entitiesDto;
    }
}