using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Transactions.Queries;

public class GetAllTransactionsForAccount
{
    public record Command(
        Guid UserId,
        int AccountId
    ) : IRequest<List<TransactionDto>>;

    public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Command, List<TransactionDto>>
    {
        public async Task<List<TransactionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var entities = await dbContext.Transactions
                .Where(transaction => transaction.Account.OwnerId == request.UserId && transaction.AccountId == request.AccountId)
                .ProjectTo<TransactionDto>(mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return entities;
        }
    }

}