using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Accounts.Commands;

public static class CreateAccount
{
    public record Command : IRequest<AccountDto>
    {
        public Guid OwnerId { get; set; }
        public AccountType AccountType { get; init; }
        public CurrencyService.Currencies Currency { get; init; }
        public decimal InterestRate { get; init; }
    }

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Command, AccountDto>
    {
        public override async Task<AccountDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                OwnerId = request.OwnerId,
                AccountType = request.AccountType,
                Currency = request.Currency,
                InterestRate = request.InterestRate,
                OpenDate = DateTime.Now
            };

            await dbDbContext.Accounts.AddAsync(account, cancellationToken);
            await dbDbContext.SaveChangesAsync(cancellationToken);

            return mapper.Map<AccountDto>(account);
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
            RuleFor(command => command.InterestRate).GreaterThanOrEqualTo(0)
                .When(command => command.AccountType is AccountType.Deposit or AccountType.Credit);
            RuleFor(command => command.InterestRate).Equal(0)
                .When(command => command.AccountType is AccountType.Checking);
        }
    }
}