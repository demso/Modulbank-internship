using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

[Route("api/[controller]")]
public class AccountController(IMapper mapper) : CustomController
{
    [HttpGet("{ownerId:guid}")]
    public async Task<ActionResult<List<AccountDto>>> GetAllAccounts(Guid ownerId)
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

    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var query = mapper.Map<CreateAccount.Command>(createAccountDto);
        var accountId = await Mediator.Send(query);
        return Ok(accountId);
    }
}