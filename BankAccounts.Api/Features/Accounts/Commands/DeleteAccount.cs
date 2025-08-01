using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class DeleteAccount
{
    public record Command(
        Guid OwnerId,
        int AccountId
    ) : IRequest<Unit>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await dbDbContext.Accounts.FindAsync(request.AccountId, cancellationToken);

            if (account == null || account.OwnerId != request.OwnerId)
                throw new NotFoundException(nameof(Account), request.AccountId, "У вас нет такого счета.");

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