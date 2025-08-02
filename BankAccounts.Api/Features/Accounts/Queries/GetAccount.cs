using AutoMapper;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Accounts.Queries;

public static class GetAccount
{
    public record Query(
        Guid OwnerId,
        int AccountId
    ) : IRequest<AccountDto>;
    
    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Query, AccountDto>
    {
        public override async Task<AccountDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            return mapper.Map<AccountDto>(account);
        }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
            RuleFor(command => command.AccountId).GreaterThan(0);
        }
    }
}

