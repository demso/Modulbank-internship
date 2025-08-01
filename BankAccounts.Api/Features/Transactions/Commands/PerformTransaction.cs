using System.ComponentModel.DataAnnotations;
using AutoMapper;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands;

public class PerformTransaction
{
    public record Command(
        int AccountId,
        TransactionType TransactionType,
        decimal Amount,
        string Description
    ) : IRequest<TransactionDto>;

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : IRequestHandler<Command, TransactionDto>
    {
        public async Task<TransactionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            
            var account = await dbDbContext.Accounts.FindAsync(request.AccountId, cancellationToken);
            if (account is null)
                throw new Exception($"Счет с id = {request.AccountId} не найден.");

            if (request.Amount <= 0)
                throw new Exception("Количество переводимых средств должно быть больше нуля.");

            if (request.TransactionType == TransactionType.Debit)
                account.Balance += request.Amount;
            if (request.TransactionType == TransactionType.Credit)
                account.Balance -= request.Amount;

            var transaction = new Transaction()
            {
                AccountId = account.AccountId,
                Amount = request.Amount,
                Currency = account.Currency,
                TransactionType = request.TransactionType,
                DateTime = DateTime.Now,
                Description = request.Description
            };

            await dbDbContext.Transactions.AddAsync(transaction, cancellationToken);
            await dbDbContext.SaveChangesAsync(cancellationToken);

            return mapper.Map<TransactionDto>(transaction);
        }
    }
}