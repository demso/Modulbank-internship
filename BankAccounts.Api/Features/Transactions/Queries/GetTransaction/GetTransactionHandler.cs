using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;

namespace BankAccounts.Api.Features.Transactions.Queries.GetTransaction;

/// <summary>
/// Обработчик команды.
/// </summary>>
public class GetTransactionHandler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, IMapper mapper) : BaseRequestHandler<GetTransactionQuery, TransactionDto>
{
    /// <summary>
    /// Обрабатывает команду.
    /// Выбрасывает исключение, если транзакция не найдена.
    /// </summary>
    /// <exception cref="NotFoundException"></exception>
    public override async Task<TransactionDto> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await transactionsRepository.GetByIdAsync(request.TransactionId, cancellationToken);

        if (transaction == null)
            throw new NotFoundException(nameof(Transaction), request.TransactionId);

        await GetValidAccount(accountsRepository, transaction.AccountId, request.OwnerId, cancellationToken);

        return mapper.Map<TransactionDto>(transaction);
    }
}