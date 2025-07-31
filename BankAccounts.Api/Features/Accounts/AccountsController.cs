using AutoMapper;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using BankAccounts.Api.Features.Transactions.Commands;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Features.Transactions.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

[Route("api/accounts")]
public class AccountsController(IMapper mapper) : CustomControllerBase
{
    [HttpGet]
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
        var accountId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), accountId);
    }

    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TransactionDto>>> GetAllTransaction(
        [FromBody] GetTransactionForAccountDto getTransactionForAccountDto)
    {
        var query = new GetTransactionsForAccount.Query((int)getTransactionForAccountDto.AccountId!, 
            getTransactionForAccountDto.FromDate, 
            getTransactionForAccountDto.ToDate);
        var transactionList = await Mediator.Send(query);
        return Ok(transactionList);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid transactionId)
    {
        var query = new GetTransaction.Query(transactionId);
        var transaction = await Mediator.Send(query);
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> PerformTransaction([FromBody] PerformTransactionDto performTransactionDto)
    {
        var command = mapper.Map<PerformTransaction.Command>(performTransactionDto);
        var transactionId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetTransaction), transactionId);
    }

    [HttpPost("transfer")]
    public async Task<ActionResult> PerformTransfer([FromBody] PerformTransferDto performTransferDto)
    {
        var command = mapper.Map<PerformTransfer.Command>(performTransferDto);
        await Mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), performTransferDto.FromAccountId);
    }
}

