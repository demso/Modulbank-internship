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
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable GrammarMistakeInComment
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1573 // Parameters not in comment

namespace BankAccounts.Api.Features.Accounts;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class AccountsController(IMapper mapper, IMediator mediator) : CustomControllerBase
{
    /// <summary>
    /// Creates account for current user.
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;AccountDto&gt;</returns>
    /// <response code="201">Success</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User is unauthorized</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<MbResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var command = mapper.Map<CreateAccount.Command>(createAccountDto);
        command.OwnerId = GetUserGuid();
        var result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Gets all accounts for current user.
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;List&lt;AccountDto&gt;&gt;</returns>
    /// <response code="200">Success</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="404">Account do not exist or belongs to another user</response>
    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<List<AccountDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<List<AccountDto>>> GetAllAccounts()
    {
        var query = new GetAllAccountsForUser.Query(GetUserGuid());
        var accountList = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, accountList);
    }

    /// <summary>
    /// Gets account of current user with id.
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/{id:int} </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;AccountDto&gt;</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="404">Account do not exist or belongs to another user</response>
    [HttpGet("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<AccountDto>> GetAccount(int accountId)
    {
        var query = new GetAccount.Query(GetUserGuid(), accountId);
        var account = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, account);
    }

    /// <summary>
    /// Deletes account (not supported).
    /// </summary>
    /// <remarks>
    /// <code>
    /// DELETE {{address}}/api/accounts/{accountId:int} </code>
    /// </remarks>
    /// <returns>Returns MbResult</returns>
    /// <response code="400">Not supported</response>
    /// <response code="401">User is unauthorized</response>
    [HttpDelete("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<MbResult> DeleteAccount(int accountId)
    {
        throw new NotSupportedException("Не стоить удалять счет, лучше его закрыть. Используйте PATCH https://.../?close=true.");
        var command = new DeleteAccount.Command(GetUserGuid(), accountId);
        await mediator.Send(command);
        return Success(StatusCodes.Status204NoContent);
    }

    /// <summary>
    /// Updates user account information. Is used to close account or change interest rate.
    /// </summary>
    /// <remarks>
    /// <code>
    /// PATCH {{address}}/api/accounts/{accountId:int} </code>
    /// </remarks>
    /// <param name="interestRate">Interest rate (>= 0)</param>
    /// <param name="close">True to close account</param>
    /// <returns>Returns MbResult</returns>
    /// <response code="204">Success</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="404">Account do not exist or belongs to another user</response>
    [HttpPatch("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult> UpdateAccount(int accountId, [FromQuery] decimal? interestRate, [FromQuery] bool close)
    {
        var command = new UpdateAccount.Command(GetUserGuid(), accountId, interestRate, close);
        await mediator.Send(command);
        return Success(StatusCodes.Status204NoContent);
    }

    /// <summary>
    /// Performs transaction.
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts/transactions </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="201">Success</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="404">Account do not exist or belongs to another user</response>
    [HttpPost("transactions")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<TransactionDto>> PerformTransaction([FromBody] PerformTransactionDto performTransactionDto)
    {
        var command = mapper.Map<PerformTransaction.Command>(performTransactionDto);
        command.OwnerId = GetUserGuid();
        var result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Performs money transfer from one account to another.
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts/transfer </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="201">Success</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="404">From account owner do not exist or belongs to another user</response>
    [HttpPost("transfer")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<TransactionDto>> PerformTransfer([FromBody] PerformTransferDto performTransferDto)
    {
        var command = mapper.Map<PerformTransfer.Command>(performTransferDto);
        command.OwnerId = GetUserGuid();
        var result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Performs money transfer from one account to another.
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/{accountId:int}/transactions </code>
    /// </remarks>
    /// <param name="fromDate">Begin of date period (DateOnly YYYY-mm-dd)</param>
    /// <param name="toDate">End of date period (DateOnly YYYY-mm-dd)</param>
    /// <returns>Returns MbResult&lt;List&lt;TransactionDto&gt;&gt;</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="404">From account owner do not exist or belongs to another user</response>
    [HttpGet("{accountId:int}/transactions")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<List<TransactionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<List<TransactionDto>>> GetTransactionsForAccount(int accountId,
        [FromQuery] DateOnly? fromDate, DateOnly? toDate)
    {
        var query = new GetTransactionsForAccount.Query(GetUserGuid(), accountId, fromDate, toDate);
        var transactionList = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, transactionList);
    }

    /// <summary>
    /// Gets transaction info by guid.
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/transactions/{transactionId:guid} </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="404">Account do not exist or belongs to another user</response>
    [HttpGet("transactions/{transactionId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<TransactionDto>> GetTransaction(Guid transactionId)
    {
        var query = new GetTransaction.Query(GetUserGuid(), transactionId);
        var transaction = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, transaction);
    }
}

