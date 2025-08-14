using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Tests.Unit.Repositories;

/// <summary>
/// Проверяет работу репозитория <seealso cref="AccountsRepositoryAsync"/>
/// </summary>
public class AccountsRepositoryTests : IDisposable
{
    private readonly BankAccountsDbContext _context;
    private readonly AccountsRepositoryAsync _repository;
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public AccountsRepositoryTests()
    {
        
        DbContextOptions<BankAccountsDbContext> options = new DbContextOptionsBuilder<BankAccountsDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        _context = new BankAccountsDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new AccountsRepositoryAsync(_context);
    }

    /// <summary>
    /// Проверяет, добавляет ли метод AddAsync репозитория счет в базу данных
    /// </summary>
    [Fact]
    public async Task AddAsync_AddAccountToDatabase_Account()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        Account account = new()
        {
            OwnerId = ownerId,
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub,
            InterestRate = 2.5m,
            OpenDate = DateTime.UtcNow.AddDays(-10)
        };

        // Act
        Account addedAccount = await _repository.AddAsync(account.OwnerId, account.AccountType, account.Currency, account.InterestRate, CancellationToken.None);
        Account? accountFromDb = await _context.Accounts.FindAsync(addedAccount.AccountId);
        
        // Assert
        addedAccount.Should().NotBeNull();
        addedAccount.AccountId.Should().BeGreaterThan(0);
        addedAccount.OwnerId.Should().Be(ownerId);
        addedAccount.InterestRate.Should().Be(2.5m); 
        
        accountFromDb.Should().NotBeNull();
        accountFromDb.OwnerId.Should().Be(ownerId);
        accountFromDb.AccountId.Should().BeGreaterThan(0);
        accountFromDb.InterestRate.Should().Be(2.5m); 
    }

    /// <summary>
    /// Проверяет, возвращает ли метод GetByIdAsync id счета, имеющегося в базе данных
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnIfExists_Account()
    {
        // Arrange
        Account account = new()
        {
            OwnerId = Guid.NewGuid(),
            AccountType = AccountType.Deposit,
            Currency = Currencies.Eur,
            InterestRate = 5.0m,
            Balance = 500.0m,
            OpenDate = DateTime.UtcNow.AddDays(-30)
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        Account? result = await _repository.GetByIdAsync(account.AccountId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(account.AccountId);
        result.OwnerId.Should().Be(account.OwnerId);
        result.Currency.Should().Be(Currencies.Eur);
        result.InterestRate.Should().Be(5.0m);
        result.Balance.Should().Be(500.0m);
    }

    /// <summary>
    /// Проверяет, что метод GetByIdAsync возвращает null, если счета нет в бд
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ReturnIfNotExists_Null()
    {
        // Act
        Account? result = await _repository.GetByIdAsync(99999, CancellationToken.None); // Non-existing ID

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract В исключительных случаях null
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}