using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class DeleteAccount
{
    public record Command(
        Guid OwnerId,
        int AccountId
    ) : IRequest<Unit>;

    public class Handler(IBankAccountsDbContext dbDbContext) : BaseRequestHandler<Command, Unit>
    {
        public override async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            if (account.Balance > 0)
                throw new Exception("Невозможно удалить счет пока баланс больше 0.");

            dbDbContext.Accounts.Remove(account);
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
        }
    }
}