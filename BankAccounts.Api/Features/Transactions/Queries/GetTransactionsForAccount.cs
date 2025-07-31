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
        DateTime? FromDate,
        DateTime? ToDate
    ) : IRequest<List<TransactionDto>>;

    public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Query, List<TransactionDto>>
    {
        public async Task<List<TransactionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await dbContext.Accounts.FindAsync(request.AccountId);
            if (account == null)
                throw new NotFoundException(nameof(Account), request.AccountId);

            var entities = await dbContext.Transactions
                .Where(transaction => transaction.AccountId == request.AccountId
                    && (request.FromDate == null || transaction.DateTime.Date >= request.FromDate.Value.Date) 
                    && (request.ToDate == null || transaction.DateTime.Date < request.ToDate.Value.Date))
                .ProjectTo<TransactionDto>(mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities;
        }
    }

}