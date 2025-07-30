using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Accounts.Queries;

public class GetAllAccountsForUser
{
    public record Query(Guid UserId) : IRequest<List<AccountDto>>;

    public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Query, List<AccountDto>>
    {
        public async Task<List<AccountDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entities = await dbContext.Accounts
                .Where(account => account.OwnerId == request.UserId)
                .ProjectTo<AccountDto>(mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities;
        }
    }
}