using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class UpdateAccount
{
    public record Command(
        Guid OwnerId,
        int AccountId,
        decimal? InterestRate,
        bool? Close
    ) : IRequest<Unit>;

    public class Handler(IBankAccountsDbContext dbDbContext) : BaseRequestHandler<Command, Unit>
    {
        public override async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            if (account.CloseDate == null && request.InterestRate.HasValue)
                account.InterestRate = request.InterestRate.Value;

            if (account.CloseDate == null && request.Close.HasValue && request.Close.Value)
            {
                if (account.Balance != 0)
                    throw new Exception("Невозможно закрыть счет на котором есть деньги.");
                account.CloseDate = DateTime.Now;
            }

            dbDbContext.Accounts.Update(account);
            await dbDbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
            RuleFor(command => command.AccountId).GreaterThan(0);
            RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
        }
    }
}


