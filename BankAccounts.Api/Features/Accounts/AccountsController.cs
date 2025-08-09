using System.Security.Claims;
using AutoMapper;
using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Accounts.Commands.CreateAccount;
using BankAccounts.Api.Features.Accounts.Commands.UpdateAccount;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries.GetAccount;
using BankAccounts.Api.Features.Accounts.Queries.GetAllAccountsForUser;
using BankAccounts.Api.Features.Accounts.Queries.GetBankStatement;
using BankAccounts.Api.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

/// <summary>
/// Контроллер операций со счетами 
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class AccountsController(IMapper mapper, IMediator mediator) : CustomControllerBase
{
    /// <summary>
    /// Открывает счет для пользователя
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts </code>
    /// </remarks>
    /// <returns>MbResult&lt;AccountDto&gt;</returns>
    /// <response code="201">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<MbResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var command = mapper.Map<CreateAccountCommand>(createAccountDto);
        command.OwnerId = GetUserGuid();
        var result = await mediator.Send(command);
        return Success(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Возвращает информацию обо всех счетах пользователя
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;List&lt;AccountDto&gt;&gt;</returns>
    /// <response code="200">Успешно</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<List<AccountDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<List<AccountDto>>> GetAllAccounts()
    {
        var query = new GetAllCountsForUserQuery(GetUserGuid());
        var accountList = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, accountList);
    }

    /// <summary>
    /// Возвращает информацию об определенном счете
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/{id:int} </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;AccountDto&gt;</returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
    [HttpGet("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<AccountDto>> GetAccount(int accountId)
    {
        var query = new GetAccountQuery(GetUserGuid(), accountId);
        var account = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, account);
    }

    /// <summary>
    /// Обновляет информацию о счете пользователя. Используется чтобы закрыть счет или поменять процентную ставку на счету.
    /// </summary>
    /// <remarks>
    /// <code>
    /// PATCH {{address}}/api/accounts/{accountId:int} </code>
    /// </remarks>
    /// <param name="interestRate">Процентная ставка (>= 0)</param>
    /// <param name="close">True закрывает аккаунт</param>
    /// /// <param name="accountId">Id счета</param>
    /// <returns>MbResult</returns>
    /// <response code="204">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
    [HttpPatch("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult> UpdateAccount(int accountId, [FromQuery] decimal? interestRate, [FromQuery] bool close)
    {
        var command = new UpdateAccountCommand(GetUserGuid(), accountId, interestRate, close);
        await mediator.Send(command);
        return Success(StatusCodes.Status204NoContent);
    }

    /// <summary>
    /// Возвращает банковскую выписку об операциях по счету
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/{accountId:int}/statement </code>
    /// </remarks>
    /// <param name="accountId">Id аккаунта</param>
    /// <param name="fromDate">Начало периода</param>
    /// <param name="toDate">Конец периода</param>
    /// <returns>MbResult&lt;BankStatement&gt;</returns>
    [HttpGet("{accountId:int}/statement")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult<BankStatement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status404NotFound)]
    public async Task<MbResult<BankStatement>> GetStatementForAccount(int accountId,
        [FromQuery] DateOnly? fromDate, DateOnly? toDate)
    {
        var query = new GetBankStatementQuery(GetUserGuid(), User.FindFirst(ClaimTypes.Name)?.Value!,
            accountId, fromDate, toDate);
        var bankStatement = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, bankStatement);
    }
}

