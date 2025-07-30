using BankAccounts.Api.Features.Accounts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

public class AccountController : CustomController
{
    [HttpGet]
    public async Task<ActionResult<AccountDto>> GetAllForUser(Guid OwnerId)
    {
        var query = ne
    }
    [HttpGet]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid AccountId)
    {
        var query = ne
    }
}