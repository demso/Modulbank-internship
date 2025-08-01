using System.Security.Claims;
using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using BankAccounts.Api.Features.Transactions.Commands;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IMapper mapper, IMediator mediator) : ControllerBase
{
    [HttpGet("all")]
    [Authorize]
    public async Task<ActionResult<List<AccountDto>>> GetAllAccounts()
    {
        var ownerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetAllAccountsForUser.Query(ownerId);
        var accountList = await mediator.Send(query);
        return Ok(accountList);
    }

    [HttpGet("{accountId:int}")]
    [Authorize]
    public async Task<ActionResult<AccountDto>> GetAccount(int accountId)
    {
        var ownerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var query = new GetAccount.Query(ownerId, accountId);
        var account = await mediator.Send(query);
        return Ok(account);
    }

    [HttpDelete("{accountId:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteAccount(int accountId)
    {
        var ownerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new DeleteAccount.Command(ownerId, accountId);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("{accountId:int}")]
    [Authorize]
    public async Task<ActionResult> UpdateAccount(int accountId, [FromQuery] decimal? interestRate, [FromQuery] bool close)
    {
        var ownerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var command = new UpdateAccount.Command(ownerId, accountId, interestRate, close);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var command = mapper.Map<CreateAccount.Command>(createAccountDto);
        command.OwnerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { accountId = result.AccountId}, result);
    }

    [HttpGet("{accountId}/transactions")]
    [Authorize]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactionsForAccount(int accountId,
        [FromBody] GetTransactionForAccountDto getTransactionForAccountDto)
    {
        var query = new GetTransactionsForAccount.Query(accountId, 
            getTransactionForAccountDto.FromDate, 
            getTransactionForAccountDto.ToDate);
        var transactionList = await mediator.Send(query);
        return Ok(transactionList);
    }

    [HttpGet("transactions/{transactionId:guid}")]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid transactionId)
    {
        var query = new GetTransaction.Query(transactionId);
        var transaction = await mediator.Send(query);
        return Ok(transaction);
    }

    [HttpPost("transactions")]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> PerformTransaction([FromBody] PerformTransactionDto performTransactionDto)
    {
        var command = mapper.Map<PerformTransaction.Command>(performTransactionDto);
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTransaction), new {transactionId = result.TransactionId}, result);
    }

    [HttpPost("transfer")]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> PerformTransfer([FromBody] PerformTransferDto performTransferDto)
    {
        var command = mapper.Map<PerformTransfer.Command>(performTransferDto);
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { accountId = result.AccountId}, result);
    }
}

