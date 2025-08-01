using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class DeleteAccount
{
    public record Command(
        int AccountId
    ) : IRequest;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await dbDbContext.Accounts.FindAsync(request.AccountId, cancellationToken);
            if (account == null)
                throw new NotFoundException(nameof(Account), request.AccountId);
            if (account.Balance > 0)
                throw new Exception("Невозможно ужалить счет пока баланс больше 0.");
            dbDbContext.Accounts.Remove(account);
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