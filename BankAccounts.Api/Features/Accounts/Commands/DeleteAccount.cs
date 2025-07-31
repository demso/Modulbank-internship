using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BankAccounts.Api.Features.Accounts.Commands;

public class DeleteAccount
{
    public record Command(
        int AccountId
    ) : IRequest;

    public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await dbContext.Accounts.FindAsync(request.AccountId, cancellationToken);
            if (account == null)
                throw new NotFoundException(nameof(Account), request.AccountId);
            if (account.Balance > 0)
                throw new ValidationException("Невозможно ужалить счет пока баланс больше 0.");
            dbContext.Accounts.Remove(account);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator(IBankAccountsContext dbContext)
        {
            RuleFor(command => command.AccountId).GreaterThan(0);
        }
    }
}