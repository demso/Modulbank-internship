using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Api.Infrastructure.Repository;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;
using BankAccounts.Api.Infrastructure.UserBlacklist;

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;

/// <summary>
/// Обработчик команды <see cref="PerformTransactionCommand"/>
/// </summary>
public class PerformTransactionHandler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, 
    IMapper mapper, IUserBlacklistService blacklist) : RequestHandlerBase<PerformTransactionCommand, TransactionDto>
{
    private static readonly Guid CausationId = CausationIds.PerformTransaction;
    
    /// <inheritdoc />
    public override async Task<TransactionDto> Handle(PerformTransactionCommand request, CancellationToken cancellationToken)
    {
        await using ISimpleTransactionScope dbTransaction = await transactionsRepository.BeginSerializableTransactionAsync(cancellationToken);
        
        try
        {
            Account account = await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);
            
            if (await blacklist.IsBlacklisted(account.OwnerId))
                throw new UserInBlockListException(request.OwnerId);
            
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault Решарпер предлагает непонятный код
            switch (request.TransactionType) {
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

            Transaction transaction = await transactionsRepository.AddAsync(
                account.AccountId, 
                0, 
                request.Amount, 
                account.Currency, 
                request.TransactionType, 
                request.Description, 
                cancellationToken);

            switch (request.TransactionType) {
                case TransactionType.Debit:
                    await transactionsRepository.AddToOutboxAsync(new MoneyCredited
                    {
                        Amount = request.Amount,
                        AccountId = transaction.AccountId,
                        Currency = transaction.Currency,
                        OperationId = transaction.TransactionId,
                        Meta = new Metadata
                        {
                            CausationId = CausationId
                        }
                    }, cancellationToken);
                    break;
                case TransactionType.Credit:
                    await transactionsRepository.AddToOutboxAsync(new MoneyDebited
                    {
                        Amount = request.Amount,
                        AccountId = transaction.AccountId,
                        Currency = transaction.Currency,
                        OperationId = transaction.TransactionId,
                        Meta = new Metadata
                        {
                            CausationId = CausationId
                        }
                    }, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request), request.TransactionType, null);
            }

            await transactionsRepository.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return mapper.Map<TransactionDto>(transaction);
        }
        catch (Exception ex)
        {
            const string message = "Transaction not performed due to an error. ";
            throw new PerformTransactionException(message, ex);
        }
    }
}