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
public class AccountsController(IMapper mapper, IMediator mediator) : CustomControllerBase
{
    [HttpGet("all")]
    [Authorize]
    public async Task<MbResult<List<AccountDto>>> GetAllAccounts()
    {
        var query = new GetAllAccountsForUser.Query(GetUserGuid());
        var accountList = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, accountList);
    }

    [HttpGet("{accountId:int}")]
    [Authorize]
    public async Task<MbResult<AccountDto>> GetAccount(int accountId)
    {
        var query = new GetAccount.Query(GetUserGuid(), accountId);
        var account = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, account);
    }

    [HttpDelete("{accountId:int}")]
    [Authorize]
    public async Task<MbResult> DeleteAccount(int accountId)
    {
        var command = new DeleteAccount.Command(GetUserGuid(), accountId);
        await mediator.Send(command);
        return Success(StatusCodes.Status204NoContent);
    }

    [HttpPatch("{accountId:int}")]
    [Authorize]
    public async Task<MbResult> UpdateAccount(int accountId, [FromQuery] decimal? interestRate, [FromQuery] bool close)
    {
        var command = new UpdateAccount.Command(GetUserGuid(), accountId, interestRate, close);
        await mediator.Send(command);
        return Success(StatusCodes.Status204NoContent);
    }

    [HttpPost]
    [Authorize]
    public async Task<MbResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var command = mapper.Map<CreateAccount.Command>(createAccountDto);
        command.OwnerId = GetUserGuid();
        var result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    [HttpGet("{accountId:int}/transactions")]
    [Authorize]
    public async Task<MbResult<List<TransactionDto>>> GetTransactionsForAccount(int accountId,
        [FromQuery] DateOnly? fromDate, DateOnly? toDate)
    {
        var query = new GetTransactionsForAccount.Query(GetUserGuid(), accountId, fromDate, toDate);
        var transactionList = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, transactionList);
    }

    [HttpGet("transactions/{transactionId:guid}")]
    [Authorize]
    public async Task<MbResult<TransactionDto>> GetTransaction(Guid transactionId)
    {
        var query = new GetTransaction.Query(GetUserGuid(), transactionId);
        var transaction = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, transaction);
    }

    [HttpPost("transactions")]
    [Authorize]
    public async Task<MbResult<TransactionDto>> PerformTransaction([FromBody] PerformTransactionDto performTransactionDto)
    {
        var command = mapper.Map<PerformTransaction.Command>(performTransactionDto);
        command.OwnerId = GetUserGuid();
        var result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    [HttpPost("transfer")]
    [Authorize]
    public async Task<MbResult<TransactionDto>> PerformTransfer([FromBody] PerformTransferDto performTransferDto)
    {
        var command = mapper.Map<PerformTransfer.Command>(performTransferDto);
        command.OwnerId = GetUserGuid();
        var result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }
}

