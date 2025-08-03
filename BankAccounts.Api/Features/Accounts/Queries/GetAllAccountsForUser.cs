using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Accounts.Queries;

public static class GetAllAccountsForUser
{
    public record Query(Guid OwnerId) : IRequest<List<AccountDto>>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Query, List<AccountDto>>
    {
        public override async Task<List<AccountDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entities = await dbDbContext.Accounts
                .Where(account => account.OwnerId == request.OwnerId)
                .ProjectTo<AccountDto>(mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities;
        }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(command => command.OwnerId).NotEqual(Guid.Empty);
        }
    }
}