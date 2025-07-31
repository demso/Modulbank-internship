using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class CreateAccount
{
    public record Command(
        Guid OwnerId,
        AccountType AccountType,
        CurrencyService.Currencies Currency,
        decimal InterestRate
    ) : IRequest<AccountDto>;

    public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Command, AccountDto>
    {
        public async Task<AccountDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.OwnerId == Guid.Empty)
                throw new Exception("Поле OwnerId в CreateAccount.Command не должно быть пустым Guid.");

            var account = new Account
            {
                OwnerId = request.OwnerId,
                AccountType = request.AccountType,
                Currency = request.Currency,
                InterestRate = request.InterestRate,
                OpenDate = DateTime.Now
            };

            await dbContext.Accounts.AddAsync(account, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return mapper.Map<AccountDto>(account);
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator(IBankAccountsContext dbContext)
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
            RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0);
        }
    }
}