using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class UpdateAccount
{
    public record Command(
        Guid OwnerId,
        int AccountId,
        decimal? InterestRate,
        bool? Close
    ) : IRequest<Unit>;

    public class Handler(IBankAccountsDbContext dbDbContext) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await dbDbContext.Accounts.FindAsync(request.AccountId);
            if (account == null || account.OwnerId != request.OwnerId)
                throw new NotFoundException(nameof(Account), request.AccountId, "У вас нет такого счета.");

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
        public CommandValidator(IBankAccountsDbContext dbDbContext)
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
            RuleFor(command => command.AccountId).GreaterThan(0);
            RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
        }
    }
}


