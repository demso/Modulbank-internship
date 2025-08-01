using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Accounts.Queries
{
    public static class GetAccount
    {
        public record Query(
            int AccountId,
            Guid OwnerId
        ) : IRequest<AccountDto>;
        
        public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : IRequestHandler<Query, AccountDto>
        {
            public async Task<AccountDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var entity = await dbDbContext.Accounts.FirstOrDefaultAsync(account => 
                    account.AccountId == request.AccountId && account.OwnerId == request.OwnerId, cancellationToken);
                if (entity == null || !request.OwnerId.Equals(entity.OwnerId))
                {
                    throw new NotFoundException(nameof(Account), request.AccountId);
                }

                return mapper.Map<AccountDto>(entity);
            }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator(IBankAccountsDbContext dbDbContext)
            {
                RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
                RuleFor(command => command.AccountId).GreaterThan(0);
            }
        }
    }
}
