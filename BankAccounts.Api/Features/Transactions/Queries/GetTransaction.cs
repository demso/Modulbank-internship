using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Features.Transactions.Queries;

public static class GetTransaction
{
    public record Query(Guid TransactionId) : IRequest<TransactionDto>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : IRequestHandler<Query, TransactionDto>
    {
        public async Task<TransactionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = await dbDbContext.Transactions.FirstOrDefaultAsync(transaction =>
                transaction.TransactionId == request.TransactionId, cancellationToken);
            if (entity == null)
            {
                throw new NotFoundException(nameof(Transaction), request.TransactionId);
            }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.TransactionId).NotEmpty();
        }
    }
    
}