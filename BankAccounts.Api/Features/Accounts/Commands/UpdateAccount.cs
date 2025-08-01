using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class UpdateAccount
{
    public record Command(
        int AccountId,
        decimal? InterestRate,
        bool? Close
    ) : IRequest;

    public class Handler(IBankAccountsDbContext dbDbContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await dbDbContext.Accounts.FindAsync(request.AccountId);
            if (account == null)
                throw new NotFoundException(nameof(Account), request.AccountId);

            if (account.CloseDate == null && request.InterestRate.HasValue)
                account.InterestRate = request.InterestRate.Value;
            if (account.CloseDate == null && request.Close.HasValue && request.Close.Value)
                account.CloseDate = DateTime.Now;

            dbDbContext.Accounts.Update(account);
            await dbDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator(IBankAccountsDbContext dbDbContext)
        {
            RuleFor(command => command.AccountId).GreaterThan(0);
        }
    }
}