using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

[Route("api/user/accounts")]
public class AccountController(IMapper mapper) : CustomController
{
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAllAccounts([FromBody] GetAllAccountsForUserDto getAllAccountsForUserDto)
    {
        var query = new GetAllAccountsForUser.Query(getAllAccountsForUserDto.UserId);
        var accountList = await Mediator.Send(query);
        return Ok(accountList);
    }
    
    [HttpGet("{accountId:guid}")]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid accountId, [FromBody] GetAccountDto getAccountDto)
    {
        var query = new GetAccount.Query(accountId, getAccountDto.UserId);
        var account = await Mediator.Send(query);
        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var command = mapper.Map<CreateAccount.Command>(createAccountDto);
        var accountId = await Mediator.Send(command);
        return Ok(accountId);
    }
}