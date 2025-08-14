using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Features.Transactions.Queries.GetTransactionsForAccount;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Queries.GetBankStatement;

/// <summary>
/// Обработчик запроса <see cref="GetBankStatementQuery"/>
/// </summary>>
public class GetBankStatementHandler(IAccountsRepositoryAsync accountsRepository, IMediator mediator) : RequestHandlerBase<GetBankStatementQuery, BankStatement>
{
    /// <inheritdoc />
    public override async Task<BankStatement> Handle(GetBankStatementQuery request, CancellationToken cancellationToken)
    {
        Account account = await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);

        DateOnly toDate = request.ToDate ?? DateOnly.FromDateTime(DateTime.Now);
        // Получаем все транзакции по счету с даты request.FromDate
        List<TransactionDto> entities = await GetTransactionsFromDate(request.OwnerId, request.AccountId, request.FromDate, cancellationToken);
        // Сортируем транзакции, чтобы в начале были последние
        SortTransactionsFromLatestToEarliest(entities);
        // Сохраняем текущий (конечный) баланс
        decimal endBalance = 0m;
        // Переменная, в которой будет начальный баланс, до операций
        decimal startBalance = account.Balance;
        // Создаем коллекцию операций на основе транзакций
        List<AccountOperation> operations = [];
        bool startBalanceSet = false;
        foreach (TransactionDto transaction in entities)
        {
            decimal sum = GetTransactionSum(transaction); //вычисляем сумму транзакции на основании типа транзакции
            decimal afterOperationBalance = startBalance;

            if (DateOnly.FromDateTime(transaction.DateTime) <= toDate) // в коллекцию добавляем только операции, совершенные в указанный период
            {
                if (!startBalanceSet)
                {
                    endBalance = startBalance;
                    startBalanceSet = true;
                }
                startBalance -= sum;
                operations.Add(new AccountOperation(
                        transaction.DateTime,
                        transaction.CounterpartyAccountId,
                        sum,
                        afterOperationBalance,
                        transaction.Description
                    ));
            }
            else
                startBalance -= sum;
            
        }
        // Сортируем от тех, что произошли раньше к более поздним
       SortOperationsFromEarliestToLatest(operations);
        // Создаем банковскую выписку
        BankStatement bankStatement = new(
            account.AccountId, 
            request.Username, 
            account.Currency, 
            DateTime.UtcNow,
            operations, 
            startBalance,
            endBalance,
            request.FromDate ?? DateOnly.MinValue,
            toDate
        );

        return bankStatement;
    }

    private static decimal GetTransactionSum(TransactionDto transaction)
    {
        return transaction.Amount * (transaction.TransactionType == TransactionType.Credit ? -1 : 1);
    }

    private static void SortTransactionsFromLatestToEarliest(List<TransactionDto> transactions)
    {
        transactions.Sort(
            (transaction1, transaction2) => 
                transaction1.DateTime > transaction2.DateTime ? -1 :
                    transaction1.DateTime.Equals(transaction2.DateTime) ? 0 : 1);
    }

    private static void SortOperationsFromEarliestToLatest(List<AccountOperation> operations)
    {
        operations.Sort(
            (operation1, operation2) => 
                operation1.DateTime > operation2.DateTime ? 1 :
                    operation1.DateTime.Equals(operation2.DateTime) ? 0 : -1);
    }

    private async Task<List<TransactionDto>> GetTransactionsFromDate(Guid ownerId, int accountId, DateOnly? fromDate, CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetTransactionsForAccountQuery(ownerId, accountId, fromDate, null), cancellationToken);
    }
    
}