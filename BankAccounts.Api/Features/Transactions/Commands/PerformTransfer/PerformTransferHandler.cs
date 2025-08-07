using AutoMapper;
using BankAccounts.Api.Common.Exceptions;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;

// ReSharper disable once UnusedType.Global Класс используется посредником

namespace BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;

/// <summary>
    /// Обработчик команды. Трансфер происходит с использованием двух транзакций, одна снимает средства с исходного счета
    /// и одна зачисляет на конченый счет.
    /// </summary>
    public class PerformTransferHandler(IAccountsRepositoryAsync accountsRepository, ITransactionsRepositoryAsync transactionsRepository, ICurrencyService currencyService, IMapper mapper) : BaseRequestHandler<PerformTransferCommand, TransactionDto>
    {
        /// <inheritdoc />
        public override async Task<TransactionDto> Handle(PerformTransferCommand request,  CancellationToken cancellationToken)
        {

            var fromAccount = await GetValidAccount(accountsRepository, request.FromAccountId, request.OwnerId, cancellationToken);

            var toAccount = await accountsRepository.GetByIdAsync(request.ToAccountId, cancellationToken);

            if (toAccount is null)
                throw new AccountNotFoundException(request.ToAccountId);

            var transactionFrom = await transactionsRepository.AddAsync(
                fromAccount.AccountId,
                toAccount.AccountId,
                request.Amount,
                fromAccount.Currency,
                TransactionType.Credit,
               $"Transaction from {fromAccount.AccountId} account to {toAccount.AccountId}.", cancellationToken);

            fromAccount.Balance -= request.Amount;
            // Если необходимо, конвертируем валюту в валюту конечного счета
            var toAccountAmount = currencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency);

            await transactionsRepository.AddAsync(
                toAccount.AccountId,
                fromAccount.AccountId,
                toAccountAmount,
                 toAccount.Currency,
                TransactionType.Debit,
                $"Transaction to {toAccount.AccountId} account from {fromAccount.AccountId}.", cancellationToken);

            toAccount.Balance += toAccountAmount;

            await transactionsRepository.SaveChangesAsync(cancellationToken);
            await accountsRepository.SaveChangesAsync(cancellationToken);

            return mapper.Map<TransactionDto>(transactionFrom);
        }
    }