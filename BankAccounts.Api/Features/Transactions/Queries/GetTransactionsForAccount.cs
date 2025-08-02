using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Transactions.Queries;

public static class GetTransactionsForAccount
{
    public record Query(
        Guid OwnerId,
        int AccountId,
        DateOnly? FromDate,
        DateOnly? ToDate
    ) : IRequest<List<TransactionDto>>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Query, List<TransactionDto>>
    {
        public override async Task<List<TransactionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            var entities = await dbDbContext.Transactions
                .Where(transaction => transaction.AccountId == request.AccountId
                    && (request.FromDate == null || DateOnly.FromDateTime(transaction.DateTime) >= request.FromDate.Value) 
                    && (request.ToDate == null || DateOnly.FromDateTime(transaction.DateTime) < request.ToDate.Value))
                .ProjectTo<TransactionDto>(mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities;
        }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(command => command.AccountId).GreaterThan(0);
            RuleFor(command => command.FromDate)
                .GreaterThan(new DateOnly(1900, 1, 1))
                .DependentRules(() =>
                    RuleFor(command => command.ToDate)
                        .GreaterThan(command => command.FromDate)
                        .WithMessage("Конец периода должен быть позже начала периода.")
                );
           
        }
    }
}