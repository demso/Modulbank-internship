using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

public class AccountController : CustomController
{
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAllForUser(Guid ownerId)
    {
        var query = new GetAllAccountsForUser.Query(ownerId);
        var accountList = await Mediator.Send(query);
        return Ok(accountList);
    }
    [HttpGet("{accountId:guid}")]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid accountId)
    {
        var query = new GetAccount.Query(accountId);
        var account = await Mediator.Send(query);
        return Ok(account);
    }
}