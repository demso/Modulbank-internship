using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Queries.GetBankStatement;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Features.Transactions.Queries.GetTransactionsForAccount;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using MediatR;
using Moq;

namespace BankAccounts.Tests.Handlers;
/// <summary>
/// Данный тест проверит функционал обработчика команды на получение банковской выписки
/// </summary>
public class GetBankStatementHandlerTests
{
    private readonly Mock<IAccountsRepositoryAsync> _mockRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly GetBankStatementHandler _handler;

    public GetBankStatementHandlerTests()
    {
        _mockRepository = new Mock<IAccountsRepositoryAsync>();
        _mockMediator = new Mock<IMediator>();
        _handler = new GetBankStatementHandler(_mockRepository.Object, _mockMediator.Object);
    }

    /// <summary>
    /// Обработчик, чтобы пройти этот тест должен вернуть выписку по счету с верными данными
    /// </summary>
    [Fact]
    public async Task Handle_ReturnValid_BankStatement()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var username = "username";
        var accountId = 1;
        var cancellationToken =CancellationToken.None;
        var request = new GetBankStatementQuery(ownerId, username, accountId, null, null);
        
        // Список транзакций для обработчика
        var transactions = new List<TransactionDto>()
        {
            new(Guid.NewGuid(), accountId, 0, 100, Currencies.Rub,
                TransactionType.Debit, null, DateTime.Now),
            new(Guid.NewGuid(), accountId, 0, 110, Currencies.Rub,
                TransactionType.Debit, null, DateTime.Now.AddMinutes(-5)),
            new(Guid.NewGuid(), accountId, 0, 120, Currencies.Rub,
                TransactionType.Credit, null, DateTime.Now.AddMinutes(5)),
            new(Guid.NewGuid(), accountId, 0, 130, Currencies.Rub,
                TransactionType.Debit, null, DateTime.Now.AddMinutes(-10)),
            new(Guid.NewGuid(), accountId, 2, 200, Currencies.Rub,
                TransactionType.Credit, null, DateTime.Now.AddMinutes(10))
        };
        
        // Операции на выходе должны быть отсортированы в порядке проведения транзакций по времени
        var operations = new List<AccountOperation>()
        {
            new(transactions[3].DateTime,
                transactions[3].CounterpartyAccountId,
                transactions[3].Amount * (transactions[3].TransactionType == TransactionType.Debit ? 1 : -1),
                130, null),
            new(transactions[1].DateTime,
                transactions[1].CounterpartyAccountId,
                transactions[1].Amount * (transactions[1].TransactionType == TransactionType.Debit ? 1 : -1),
                240, null),
            new(transactions[0].DateTime, transactions[0].CounterpartyAccountId,
                transactions[0].Amount * (transactions[0].TransactionType == TransactionType.Debit ? 1 : -1),
                340, null),
            new(transactions[2].DateTime, transactions[2].CounterpartyAccountId,
                transactions[2].Amount * (transactions[2].TransactionType == TransactionType.Debit ? 1 : -1),
                220, null),
            new(transactions[4].DateTime, transactions[4].CounterpartyAccountId,
                transactions[4].Amount * (transactions[4].TransactionType == TransactionType.Debit ? 1 : -1),
                20, null)
        };
        
        // Счет, с которым проводим операции
        Account account = new()
        {
            AccountId = accountId,
            OwnerId = ownerId,
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub,
            InterestRate = 0m,
            Balance = 20,
            OpenDate = DateTime.UtcNow
        };
        
        // Имитируем получение валидного счета в обработчике
        _mockRepository.Setup(r => r.GetByIdAsync(accountId, cancellationToken))
            .ReturnsAsync(account);
        
        // Имитируем получение списка транзакций по аккаунту в обработчике
        _mockMediator.Setup(m => m.Send(It.IsAny<GetTransactionsForAccountQuery>(), cancellationToken))
            .ReturnsAsync(transactions);
        
        // Act
        var bankStatement = await _handler.Handle(request, cancellationToken);
        
        // Assert
        Assert.Equal(accountId, bankStatement.AccountId);
        Assert.Equal(username, bankStatement.Username);
        Assert.Equal(account.Currency, bankStatement.Currency);
        Assert.Equal(operations,  bankStatement.Operations);
        _mockRepository.Verify(m => m.GetByIdAsync(accountId, cancellationToken), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<GetTransactionsForAccountQuery>(), cancellationToken), Times.Once);
    }
}
