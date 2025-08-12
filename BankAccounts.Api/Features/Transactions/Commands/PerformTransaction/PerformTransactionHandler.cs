using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;

/// <summary>
/// Обработчик команды <see cref="PerformTransactionCommand"/>
/// </summary>
public class PerformTransactionHandler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, 
    IMapper mapper) : RequestHandlerBase<PerformTransactionCommand, TransactionDto>
{
    /// <inheritdoc />
    public override async Task<TransactionDto> Handle(PerformTransactionCommand request, CancellationToken cancellationToken)
    {
        var account = await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault Решарпер предлагает непонятный код
        switch (request.TransactionType)
        {
            case TransactionType.Debit:
                account.Balance += request.Amount;
                break;
            case TransactionType.Credit:
                if (account.AccountType is AccountType.Checking or AccountType.Deposit &&
                    account.Balance - request.Amount < 0)
                    throw new BadRequestException("Баланс после транзакции не может быть < 0, т.к. счет не является кредитным.");
                account.Balance -= request.Amount;
                break;
        }

        var transaction = await transactionsRepository.AddAsync(
            account.AccountId, 
            0, 
            request.Amount, 
            account.Currency, 
            request.TransactionType, 
            request.Description, 
            cancellationToken);

        await transactionsRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<TransactionDto>(transaction);
    }
}