using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Transactions.Queries;

public static class GetTransaction
{
    public record Query(Guid TransactionId, Guid UserId) : IRequest<TransactionDto>;

    public class Handler(IBankAccountsContext dbContext, IMapper mapper) : IRequestHandler<Query, TransactionDto>
    {
        public async Task<TransactionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Transactions.FirstOrDefaultAsync(transaction =>
                transaction.TransactionId == request.TransactionId && transaction.Account.OwnerId == request.UserId, cancellationToken);
            if (entity == null || !entity.Account.OwnerId.Equals(request.UserId))
            {
                throw new NotFoundException(nameof(Transaction), request.TransactionId + " " + request.UserId);
            }

            return mapper.Map<TransactionDto>(entity);
        }
    }
    
}