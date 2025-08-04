using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database;
using FluentValidation;
using MediatR;

namespace BankAccounts.Api.Features.Transactions.Queries;
/// <summary>
/// Класс для создания бакнуовской выписки
/// </summary>
public static class GetBankStatement
{
    /// <summary>
    /// Запрос выписки
    /// </summary>
    /// <param name="OwnerId">Id владельца</param>
    /// <param name="AccountId">Id счета</param>
    /// <param name="FromDate">Начало периода (может быть null)</param>
    /// <param name="ToDate">Конец периода (может быть null)</param>
    public record Query(
        Guid OwnerId,
        string Username,
        int AccountId,
        DateOnly? FromDate,
        DateOnly? ToDate
    ) : IRequest<BankStatement>;
    /// <summary>
    /// Обработчик команды
    /// </summary>>
    public class Handler(IBankAccountsDbContext dbDbContext, IMediator mediator) : BaseRequestHandler<Query, BankStatement>
    {
        /// <inheritdoc />
        public override async Task<BankStatement> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await GetValidAccount(dbDbContext, request.AccountId, request.OwnerId, cancellationToken);

            var toDate = request.ToDate ?? DateOnly.FromDateTime(DateTime.Now);
            //получаем все транзакции с даты request.FromDate
            var entities = await mediator.Send(new GetTransactionsForAccount.Query(request.OwnerId, request.AccountId, request.FromDate, null), cancellationToken);
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

                if (DateOnly.FromDateTime(transaction.DateTime) <= toDate) // в коллекцию добавляем только операцие, совершенные в указанный период
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
                DateTime.Now,
                operations, 
                startBalance,
                endBalance,
                request.FromDate ?? DateOnly.MinValue,
                toDate
            );

            return bankStatement;
        }
    }

    /// <summary>
    /// Валидатор команды
    /// </summary>
    // ReSharper disable once UnusedType.Global Класс используется посредником
    public class QueryValidator : AbstractValidator<Query>
    {
        /// <summary>
        /// Создание валидатора и настройка правил
        /// </summary>
        public QueryValidator()
        {
            RuleFor(query => query.AccountId).GreaterThan(0);
            RuleFor(query => query.FromDate)
                .GreaterThan(new DateOnly(1900, 1, 1))
                .When(query => query.FromDate is not null)
                .DependentRules(() =>
                    RuleFor(query => query.ToDate)
                        .GreaterThan(query => query.FromDate)
                        .When(query => query.ToDate is not null)
                        .WithMessage("Конец периода должен быть позже начала периода."))
                .When(query => query.FromDate is not null);

        }
    }
    /// <summary>
    /// Операция по счету
    /// </summary>
    /// <param name="DateTime">Время проведения</param>
    /// <param name="CounterPartyId">Счет получателя (если есть)</param>
    /// <param name="Sum">Сумма</param>
    /// <param name="AfterBalance">Баланс после операции</param>
    /// <param name="Description">Описание</param>
    // ReSharper disable NotAccessedPositionalProperty.Global Свойства используются
    public record AccountOperation(
        DateTime DateTime,
        int CounterPartyId,
        decimal Sum,
        decimal AfterBalance,
        string? Description
    );
}
/// <summary>
/// ВЫписка о банковских операция по счету
/// </summary>
/// <param name="AccountId">Id аккаунта</param>
/// <param name="Username">Имя пользователя</param>
/// <param name="Currency">Валюта</param>
/// <param name="CreationDateTime">Время создания выписки</param>
/// <param name="Operations">Операции</param>
/// <param name="StartBalance">Баланс на начало периода</param>
/// <param name="EndBalance">Баланс на конец периода</param>
public record BankStatement(
    int AccountId,
    string Username,
    Currencies Currency,
    DateTime CreationDateTime,
    List<GetBankStatement.AccountOperation> Operations,
    decimal StartBalance,
    decimal EndBalance,
    DateOnly StartPeriod,
    DateOnly EndPeriod
);