using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Features.Accounts.Dtos;

public record GetAccountDto(
    [Required] Guid? OwnerId
);