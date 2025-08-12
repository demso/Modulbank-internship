using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Commands.CreateAccount;
using BankAccounts.Api.Infrastructure.CurrencyService;
using FluentValidation.TestHelper;

namespace BankAccounts.Tests.Unit.Validators;

/// <summary>
/// Тесты валидатора команды создания счета <seealso cref="CreateAccountValidator"/>
/// </summary>
public class CreateAccountValidatorTests
{
    private readonly CreateAccountValidator _validator = new();
    
    [Fact]
    public void CreateAccountValidator_ShouldHaveErrorWhenOwnerIdIsEmpty()
    {
        var command = new CreateAccountCommand() 
        {
            OwnerId = Guid.Empty,
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub, 
            InterestRate = 5.0m
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.OwnerId);
    }

    [Fact]
    public void CreateAccountValidator_ShouldNotHaveErrorWhenOwnerIdIsValid()
    {
        var command = new CreateAccountCommand()
        {
            OwnerId = Guid.NewGuid(),
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub,
            InterestRate = 5.0m
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.OwnerId);
    }

    [Fact]
    public void CreateAccountValidator_ShouldNotHaveErrorWhenInterestRateIsNegative()
    {
        var command = new CreateAccountCommand()
        {
            OwnerId = Guid.Empty,
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub,
            InterestRate = -1.0m
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.InterestRate);
    }

    [Fact]
    public void CreateAccountValidator_ShouldNotHaveErrorWhenInterestRateIsZeroOrPositive()
    {
        var command1 = new CreateAccountCommand()
        {
            OwnerId = Guid.Empty,
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub,
            InterestRate = 0m
        };
        var command2 = new CreateAccountCommand() {
            OwnerId = Guid.Empty,
            AccountType = AccountType.Checking,
            Currency = Currencies.Rub, 
            InterestRate = 5.0m
        };

        _validator.TestValidate(command1).ShouldNotHaveValidationErrorFor(c => c.InterestRate);
        _validator.TestValidate(command2).ShouldNotHaveValidationErrorFor(c => c.InterestRate);
    }
}
