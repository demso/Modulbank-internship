using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;

namespace BankAccounts.Api.Features.Accounts.Commands.CreateAccount;


/// <summary>
/// Обработчик команды
/// </summary>
public class CreateAccountHandler(IAccountsRepositoryAsync accountsRepository, IMapper mapper) : BaseRequestHandler<CreateAccountCommand, AccountDto>
{
    /// <inheritdoc />
    public override async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await accountsRepository.AddAsync(request.OwnerId, request.AccountType, request.Currency, 
            request.InterestRate, cancellationToken);

        await accountsRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<AccountDto>(account);
    }
}
