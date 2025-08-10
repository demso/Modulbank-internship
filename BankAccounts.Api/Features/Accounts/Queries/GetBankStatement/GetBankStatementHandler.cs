using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Queries.GetTransactionsForAccount;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Queries.GetBankStatement;

/// <summary>
/// Обработчик команды
/// </summary>>
public class GetBankStatementHandler(IAccountsRepositoryAsync accountsRepository, IMediator mediator) : BaseRequestHandler<GetBankStatementQuery, BankStatement>
{
    /// <inheritdoc />
    public override async Task<BankStatement> Handle(GetBankStatementQuery request, CancellationToken cancellationToken)
    {
        var account = await GetValidAccount(accountsRepository, request.AccountId, request.OwnerId, cancellationToken);

        var toDate = request.ToDate ?? DateOnly.FromDateTime(DateTime.Now);
        //получаем все транзакции с даты request.FromDate
        var entities = await mediator.Send(new GetTransactionsForAccountQuery(request.OwnerId, request.AccountId, request.FromDate, null), cancellationToken);
        //сортируем транзакции, чтобы в начале были последние
        entities.Sort((transaction1, transaction2) => transaction1.DateTime > transaction2.DateTime ? -1 :
            transaction1.DateTime.Equals(transaction2.DateTime) ? 0 : 1);
        //сохраняем текущий (конечный) баланс
        var endBalance = 0m;
        //переменная, в которой будет начальный баланс, до операций
        var startBalance = account.Balance;
        //создаем коллекцию операций на основе транзакций
        var operations = new List<AccountOperation>();
        var balanceSet = false;
        foreach (var transaction in entities)
        {
            var sum = transaction.Amount * (transaction.TransactionType == TransactionType.Credit ? -1 : 1); //вычисляем сумму транзакции на основании типа транзакции
            var afterOperationBalance = startBalance;

            if (DateOnly.FromDateTime(transaction.DateTime) <= toDate) // в коллекцию добавляем только операции, совершенные в указанный период
            {
                if (!balanceSet)
                {
                    endBalance = startBalance;
                    balanceSet = true;
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
            {
                startBalance -= sum;
            }
            
        }
        //сортируем от тех, что произошли раньше к более поздним
        operations.Sort((transaction1, transaction2) => transaction1.DateTime > transaction2.DateTime ? 1 :
            transaction1.DateTime.Equals(transaction2.DateTime) ? 0 : -1);
        //создаем банковскую выписку
        var bankStatement = new BankStatement(
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
}