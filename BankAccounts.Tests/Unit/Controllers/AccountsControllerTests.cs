using AutoMapper;
using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands.CreateAccount;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Infrastructure.CurrencyService;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankAccounts.Tests.Unit.Controllers;

/// <summary>
/// Тесты контроллера счетов
/// </summary>
public class AccountsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper; // If controller uses it directly
    private readonly AccountsController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();
    const string Username ="username";
    
    public AccountsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, Username),
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        ], "mock"));

        _controller = new AccountsController(_mockMapper.Object, _mockMediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    /// <summary>
    /// Проверит, отправит ли контроллер команду на создание счета и вернет ли Result&lt;AccountDto&gt;
    /// </summary>
    [Fact]
    public async Task CreateAccount_ReturnCreatedWith_ResultAccountDto()
    {
        // Arrange
        var createDto = new CreateAccountDto(AccountType.Deposit, Currencies.Usd, 4.5m);
        var accountDto = new AccountDto(2, AccountType.Deposit, Currencies.Usd, 4.5m, 0, DateTime.UtcNow, null);
        var mbResult = MbResult<AccountDto>.Success(StatusCodes.Status201Created, accountDto);
        var createAccountCommand = new CreateAccountCommand()
        {
            OwnerId = _testUserId,
            AccountType = AccountType.Deposit,
            Currency = Currencies.Usd,
            InterestRate = 4.5m
        };
        
        _mockMapper.Setup(x => x.Map<CreateAccountCommand>(It.IsAny<CreateAccountDto>()))
            .Returns(createAccountCommand);

        _mockMediator.Setup(m => m.Send(createAccountCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountDto);

        // Act
        var result = await _controller.CreateAccount(createDto);

        // Assert
        var createdAtActionResult = result;
        createdAtActionResult.Should().BeEquivalentTo(mbResult);
        createdAtActionResult.Should().NotBeNull();
        createdAtActionResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdAtActionResult.Value.Should().BeEquivalentTo(accountDto);

        // Verify command sent to mediator
        _mockMediator.Verify(m => m.Send(It.Is<CreateAccountCommand>(cmd =>
            cmd.OwnerId == _testUserId &&
            cmd.AccountType == createDto.AccountType &&
            cmd.Currency == createDto.Currency &&
            cmd.InterestRate == createDto.InterestRate),
            CancellationToken.None), Times.Once);
    }
    
    // Проверка авторизации (если GetUserGuid выбрасывает исключение при отсутствии пользователя)
}