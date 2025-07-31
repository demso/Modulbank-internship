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
public class AccountController(IMapper mapper) : CustomController
{
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAllAccounts([FromBody] GetAllAccountsForUserDto getAllAccountsForUserDto)
    {
        var query = new GetAllAccountsForUser.Query(getAllAccountsForUserDto.OwnerId);
        var accountList = await Mediator.Send(query);
        return Ok(accountList);
    }
    
    [HttpGet("{accountId:int}")]
    public async Task<ActionResult<AccountDto>> GetAccount(int accountId, [FromBody] GetAccountDto getAccountDto)
    {
        var query = new GetAccount.Query(accountId, getAccountDto.OwnerId);
        var account = await Mediator.Send(query);
        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var command = mapper.Map<CreateAccount.Command>(createAccountDto);
        var accountId = await Mediator.Send(command);
        return Ok(accountId);
    }
}

[Route("api/transactions")]
public class TransactionController(IMapper mapper) : CustomController
{
    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> GetAllTransaction([FromBody] GetAllTransactionForAccountDto getAllTransactionForAccountDto)
    {
        var query = new GetAllTransactionsForAccount.Query( getAllTransactionForAccountDto.AccountId);
        var transactionList = await Mediator.Send(query);
        return Ok(transactionList);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<TransactionDto>> GetAccount(Guid transactionId)
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
        return Ok(transactionId);
    }

    [HttpPost("transfer")]
    public async Task<ActionResult> PerformTransfer([FromBody] PerformTransferDto performTransferDto)
    {
        var command = mapper.Map<PerformTransfer.Command>(performTransferDto);
        await Mediator.Send(command);
        return Ok("Трансфер произведен успешно");
    }
}