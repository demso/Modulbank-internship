using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Accounts.Queries;

public static class GetAllAccountsForUser
{
    public record Query(Guid OwnerId) : IRequest<List<AccountDto>>;

    public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Query, List<AccountDto>>
    {
        public async Task<List<AccountDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entities = await dbContext.Accounts
                .Where(account => account.OwnerId == request.OwnerId)
                .ProjectTo<AccountDto>(mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities;
        }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator(IBankAccountsContext dbContext)
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
        }
    }
}