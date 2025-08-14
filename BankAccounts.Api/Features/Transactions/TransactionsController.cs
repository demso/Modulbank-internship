using AutoMapper;
using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Commands.PerformTransaction;
using BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Features.Transactions.Queries.GetTransaction;
using BankAccounts.Api.Features.Transactions.Queries.GetTransactionsForAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Transactions;

/// <summary>
/// Контроллер операций с транзакциями по счетам.
/// <seealso cref="Transaction"/>
/// </summary>
/// <param name="mapper"><see cref="Mapper"/></param>
/// <param name="mediator"><see cref="Mediator"/></param>
[ApiController]
[Produces("application/json")]
[Route("api/accounts")]
public class TransactionsController(IMapper mapper, IMediator mediator) : CustomControllerBase
{
    /// <summary>
    /// Производит транзакцию
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts/transactions </code>
    /// </remarks>
    /// <returns>MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="201">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
    [HttpPost("{accountId:int}/transactions")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<TransactionDto>> PerformTransaction(int accountId,
        [FromBody] PerformTransactionDto performTransactionDto)
    {
        PerformTransactionCommand? command = mapper.Map<PerformTransactionCommand>(performTransactionDto);
        command.OwnerId = GetUserGuid();
        command.AccountId = accountId;
        TransactionDto result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Производит трансфер денежных средств с одного счета на другой
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts/transfer </code>
    /// </remarks>
    /// <returns>MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="201">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Исходный счет не существует или не принадлежит текущему пользователю</response>
    [HttpPost("transfer")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<TransactionDto>> PerformTransfer([FromBody] PerformTransferDto performTransferDto)
    {
        PerformTransferCommand? command = mapper.Map<PerformTransferCommand>(performTransferDto);
        command.OwnerId = GetUserGuid();
        TransactionDto result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Возвращает все транзакции по счету или только за определенный период
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/{accountId:int}/transactions </code>
    /// </remarks>
    /// <param name="accountId">Id счета</param>
    /// <param name="fromDate">Начало периода (DateOnly YYYY-mm-dd nullable)</param>
    /// <param name="toDate">Конец периода (DateOnly YYYY-mm-dd nullable)</param>
    /// <returns>MbResult&lt;List&lt;TransactionDto&gt;&gt;</returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Счет не существует или не принадлежит текущему пользователю</response>
    [HttpGet("{accountId:int}/transactions")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<List<TransactionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<List<TransactionDto>>> GetTransactionsForAccount(int accountId,
        [FromQuery] DateOnly? fromDate, DateOnly? toDate)
    {
        GetTransactionsForAccountQuery query = new(GetUserGuid(), accountId, fromDate, toDate);
        List<TransactionDto> transactionList = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, transactionList);
    }

    /// <summary>
    /// Возвращает информацию о транзакции
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/transactions/{transactionId:guid} </code>
    /// </remarks>
    /// <returns>MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Счет не существует или не принадлежит текущему пользователю</response>
    [HttpGet("transactions/{transactionId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<TransactionDto>> GetTransaction(Guid transactionId)
    {
        GetTransactionQuery query = new(GetUserGuid(), transactionId);
        TransactionDto transaction = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, transaction);
    }
}