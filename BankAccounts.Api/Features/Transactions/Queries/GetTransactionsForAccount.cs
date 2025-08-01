using System.ComponentModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Transactions.Queries;

public static class GetTransactionsForAccount
{
    public record Query(
        int AccountId,
        DateOnly? FromDate,
        DateOnly? ToDate
    ) : IRequest<List<TransactionDto>>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : IRequestHandler<Query, List<TransactionDto>>
    {
        public async Task<List<TransactionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await dbDbContext.Accounts.FindAsync(request.AccountId);
            if (account == null)
                throw new NotFoundException(nameof(Account), request.AccountId);
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

}