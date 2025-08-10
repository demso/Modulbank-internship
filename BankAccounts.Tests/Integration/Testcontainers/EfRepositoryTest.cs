using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BankAccounts.Tests.Testcontainers;

/// <summary>
/// Тест работы Entity Framework с контекстом BankAccountsDbContext и базой данных Postgres
/// </summary>
public class EfRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres")
        .Build();
    private BankAccountsDbContext _context = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<BankAccountsDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new BankAccountsDbContext(options);
        await _context.Database.MigrateAsync(); 
    }
        
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _container.StopAsync();
    }

    
    /// <summary>
    /// Проверка, сможет ли Entity Framework добавить счет в базу данных
    /// </summary>
    [Fact]
    public async Task Can_Add_Account()
    {
        // Arrange
        var account = new Account
        {
            OwnerId = Guid.NewGuid(),
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub,
            InterestRate = 2.5m
        };
        
        // Act
        var returnedAccount = _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Accounts.FindAsync(returnedAccount.Entity.AccountId);
        Assert.NotNull(saved);
        Assert.Equal(2.5m, saved.InterestRate);
        Assert.Equal(account.OwnerId, saved.OwnerId);
        Assert.Equal(account.AccountType, saved.AccountType);
        Assert.Equal(account.Currency, saved.Currency);
    }
}