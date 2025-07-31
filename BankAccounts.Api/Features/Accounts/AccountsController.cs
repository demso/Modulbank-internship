using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using BankAccounts.Api.Features.Transactions.Commands;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Features.Transactions.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

[Route("api/[controller]")]
public class AccountsController(IMapper mapper) : CustomControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<List<AccountDto>>> GetAllAccounts([FromBody] GetAllAccountsForUserDto getAllAccountsForUserDto)
    {
        var query = new GetAllAccountsForUser.Query((Guid)getAllAccountsForUserDto.OwnerId!);
        var accountList = await Mediator.Send(query);
        return Ok(accountList);
    }

    [HttpGet("{accountId:int}")]
    public async Task<ActionResult<AccountDto>> GetAccount(int accountId, [FromBody] GetAccountDto getAccountDto)
    {
        var query = new GetAccount.Query(accountId, (Guid)getAccountDto.OwnerId!);
        var account = await Mediator.Send(query);
        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var command = mapper.Map<CreateAccount.Command>(createAccountDto);
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { accountId = result.AccountId}, result);
    }

    [HttpGet("{accountId}/transactions")]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactionsForAccount(int accountId,
        [FromBody] GetTransactionForAccountDto getTransactionForAccountDto)
    {
        var query = new GetTransactionsForAccount.Query(accountId, 
            getTransactionForAccountDto.FromDate, 
            getTransactionForAccountDto.ToDate);
        var transactionList = await Mediator.Send(query);
        return Ok(transactionList);
    }

    [HttpGet("transactions/{transactionId:guid}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid transactionId)
    {
        var query = new GetTransaction.Query(transactionId);
        var transaction = await Mediator.Send(query);
        return Ok(transaction);
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<Guid>> PerformTransaction([FromBody] PerformTransactionDto performTransactionDto)
    {
        var command = mapper.Map<PerformTransaction.Command>(performTransactionDto);
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetTransaction), new {transactionId = result.TransactionId}, result);
    }

    [HttpPost("transfer")]
    public async Task<ActionResult> PerformTransfer([FromBody] PerformTransferDto performTransferDto)
    {
        var command = mapper.Map<PerformTransfer.Command>(performTransferDto);
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { accountId = result.AccountId}, result);
    }
}

