﻿using AutoMapper;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Commands;

public static class PerformTransaction
{
    public record Command : IRequest<TransactionDto>
    {
        public Guid OwnerId { get; set; }
        public int AccountId { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class Handler(IBankAccountsDbContext dbDbContext, IMapper mapper) : BaseRequestHandler<Command, TransactionDto>
    {
        public override async Task<TransactionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

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

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.AccountId).GreaterThan(0);
            RuleFor(command => command.Description).MaximumLength(255);
            RuleFor(command => command.Amount).NotEqual(0);
        }
    }
}