using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Accounts.Queries
{
    public static class GetAccount
    {
        public record Query(Guid AccountId, Guid UserId) : IRequest<AccountDto>;
        
        public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Query, AccountDto>
        {
            public async Task<AccountDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var entity = await dbContext.Accounts.FirstOrDefaultAsync(account => 
                    account.AccountId == request.AccountId && account.OwnerId == request.UserId, cancellationToken);
                if (entity == null || !entity.OwnerId.Equals(entity.OwnerId))
                {
                    throw new NotFoundException(nameof(Account), request.AccountId);
                }

                return mapper.Map<AccountDto>(entity);
            }
        }
    }
}
