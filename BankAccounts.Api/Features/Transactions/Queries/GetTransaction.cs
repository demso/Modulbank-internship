using AutoMapper;
using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedType.Global

namespace BankAccounts.Api.Features.Transactions.Queries;

public static class GetTransaction
{
    public record Query(
        Guid OwnerId,
        Guid TransactionId
    ) : IRequest<TransactionDto>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Query, TransactionDto>
    {
        public override async Task<TransactionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await dbDbContext.Transactions.FirstOrDefaultAsync(transaction =>
                transaction.TransactionId == request.TransactionId, cancellationToken);

            if (transaction == null)
                throw new NotFoundException(nameof(Transaction), request.TransactionId);

            await GetValidAccount(dbDbContext, transaction.AccountId, request.OwnerId, cancellationToken);

            return mapper.Map<TransactionDto>(transaction);
        }
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