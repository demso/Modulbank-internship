using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class CreateAccount
{
    public record Command(CreateAccountDto CreateDto) : IRequest<Guid>;

    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                OwnerId = request.CreateDto.OwnerId,
                AccountType = request.CreateDto.AccountType,
                Currency = request.CreateDto.Currency,
                InterestRate = request.CreateDto.InterestRate,
                OpenDate = DateTime.Now
            };

            await dbContext.Accounts.AddAsync(account, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return account.AccountId;
        }
    }
}