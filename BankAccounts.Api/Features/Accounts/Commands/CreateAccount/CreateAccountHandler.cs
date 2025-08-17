using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Api.Infrastructure.Repository;
using BankAccounts.Api.Infrastructure.Repository.Accounts;

namespace BankAccounts.Api.Features.Accounts.Commands.CreateAccount;

/// <summary>
/// Обработчик команды <see cref="CreateAccountCommand"/>
/// </summary>
public class CreateAccountHandler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : RequestHandlerBase<CreateAccountCommand, AccountDto>
{
    private static readonly Guid CausationId = CausationIds.CreateAccount;
    
    /// <inheritdoc />
    public override async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        await using ISimpleTransactionScope dbTransaction = await accountsRepository.BeginSerializableTransactionAsync(cancellationToken);
        
        try {
            Account account = await accountsRepository.AddAsync(request.OwnerId, request.AccountType, request.Currency, 
                request.InterestRate, cancellationToken);
            
            await accountsRepository.AddToOutboxAsync(new AccountOpened
            {
                AccountId = account.AccountId, 
                OwnerId = account.OwnerId, 
                AccountType = account.AccountType, 
                Currency = account.Currency,
                Metadata = new Metadata
                {
                    CausationId = CausationId
                }
            }, cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
            
            return mapper.Map<AccountDto>(account);
        }
        catch (Exception ex)
        {
            const string message = "Account not opened due to an error. ";
            throw new CreateAccountException(message, ex);
        }
    }
}
