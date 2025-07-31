using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record GetAllAccountsForUserDto(
    [Required] Guid? OwnerId
);