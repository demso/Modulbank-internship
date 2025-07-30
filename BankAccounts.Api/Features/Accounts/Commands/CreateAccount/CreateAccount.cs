using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands.CreateAccount;

public class CreateAccount
{
    public class Command : IRequest<Guid>
    {
        public Guid OwnerId { get; set; }
        public AccountType AccountType { get; set; }
        public string Currency { get; set; }
        public decimal? InterestRate { get; set; }
    }

    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<CreateAccount.Command, Guid>
    {
        private readonly IBankAccountsContext _dbContext = dbContext;

        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                OwnerId = request.OwnerId,
                AccountType = request.AccountType,
                Currency = request.Currency,
                InterestRate = request.InterestRate,
                OpenDate = DateTime.Now
            };

            await _dbContext.Accounts.AddAsync(account, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return account.AccountId;
        }
    }
}