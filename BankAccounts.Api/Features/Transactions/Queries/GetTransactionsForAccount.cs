using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedType.Global

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
           await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

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
            RuleFor(query => query.AccountId).GreaterThan(0);
            RuleFor(query => query.FromDate)
                .GreaterThan(new DateOnly(1900, 1, 1))
                .When(query => query.FromDate is not null)
                .DependentRules(() =>
                    RuleFor(query => query.ToDate)
                        .GreaterThan(query => query.FromDate)
                        .When(query => query.ToDate is not null)
                        .WithMessage("Конец периода должен быть позже начала периода."))
                .When(query => query.FromDate is not null);
           
        }
    }
}