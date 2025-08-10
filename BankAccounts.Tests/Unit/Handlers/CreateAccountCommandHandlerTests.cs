using AutoMapper;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands.CreateAccount;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using Moq;

namespace BankAccounts.Tests.Handlers;

/// <summary>
/// Проверяет, создает ли обработчик команды на создание счета правильный счет
/// </summary>
public class CreateAccountCommandHandlerTests
{
    private readonly Mock<IAccountsRepositoryAsync> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateAccountHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _mockRepository = new Mock<IAccountsRepositoryAsync>();
        _mockMapper = new Mock<IMapper>();
        _handler = new CreateAccountHandler(_mockRepository.Object, _mockMapper.Object);
    }

    /// <summary>
    /// Обработчик должен добавить новый счет в бд и вернуть правильный DTO этого счета
    /// </summary>
    [Fact]
    public async Task Handle_CallRepositoryAddAsync_AccountDto()
    {
        var ownerId = Guid.NewGuid();
        var command = new CreateAccountCommand() 
        {
            OwnerId = ownerId, 
            AccountType = AccountType.Checking, 
            Currency = Currencies.Rub, 
            InterestRate = 3.5m
        };
        var account = new Account
        {
            AccountId = 1,
            OwnerId = ownerId,
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub,
            InterestRate = 3.5m,
            Balance = 0,
            OpenDate = DateTime.UtcNow
        };
        
        _mockRepository.Setup(r => r.AddAsync(account.OwnerId, account.AccountType, account.Currency, 
                account.InterestRate, CancellationToken.None))
            .ReturnsAsync(account); 
        
        _mockMapper.Setup(m => m.Map<AccountDto>(account))
            .Returns((Account a) => new AccountDto(a.AccountId, a.AccountType, a.Currency, a.Balance, 
                a.InterestRate, a.OpenDate, a.CloseDate));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
       Assert.NotNull(result);
       Assert.Equal(result.AccountId, account.AccountId);
       Assert.Equal(result.AccountType, account.AccountType);
       Assert.Equal(result.Currency, account.Currency);
       Assert.Equal(result.InterestRate, account.InterestRate);
       Assert.Equal(result.Balance, account.Balance);
       Assert.Equal(result.OpenDate, account.OpenDate);
       Assert.Equal(result.CloseDate, account.CloseDate);
       _mockRepository.Verify(r => r.AddAsync(account.OwnerId, account.AccountType, account.Currency,
           account.InterestRate, CancellationToken.None), Times.Once);
       _mockMapper.Verify(m => m.Map<AccountDto>(account), Times.Once);
    }
}