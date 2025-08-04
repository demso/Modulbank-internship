using AutoMapper;
using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Accounts.Commands;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Accounts.Queries;
using BankAccounts.Api.Features.Shared;
using BankAccounts.Api.Features.Transactions.Commands;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features.Accounts;

/// <summary>
/// Контроллер операций со счетами и транзакциями
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
    /// <response code="401">Пользователь неавторизован</response>
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
    /// Возвращает информацию обо всех счетах пользователя
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;List&lt;AccountDto&gt;&gt;</returns>
    /// <response code="200">Успешно</response>
    /// <response code="401">Пользователь неавторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
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
    /// Возвращает информацию об определенном счете
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/{id:int} </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;AccountDto&gt;</returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь неавторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
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
    /// Удалаяет счет (not supported)
    /// </summary>
    /// <remarks>
    /// <code>
    /// DELETE {{address}}/api/accounts/{accountId:int} </code>
    /// </remarks>
    /// <returns>MbResult</returns>
    /// <response code="400">Не поддерживается</response>
    /// <response code="401">Пользователь неавторизован</response>
    
    [HttpDelete("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(MbResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<MbResult> DeleteAccount(int accountId)
    {
        throw new NotSupportedException("Не стоить удалять счет, лучше его закрыть. Используйте PATCH https://.../?close=true.");
        // ReSharper disable once HeuristicUnreachableCode Код оставлен для примера реализации операции удаления
        #pragma warning disable CS0162 // Unreachable code detected
        var command = new DeleteAccount.Command(GetUserGuid(), accountId);
        await mediator.Send(command);
        return Success(StatusCodes.Status204NoContent);
        #pragma warning restore CS0162 // Unreachable code detected
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
    /// <response code="401">Пользователь неавторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
    [HttpPatch("{accountId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
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
    /// Производит транзакцию
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts/transactions </code>
    /// </remarks>
    /// <returns>MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="201">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь неавторизован</response>
    /// <response code="404">Счет не существует или не принадлежит пользователю</response>
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
    /// Производит трансфер денежных средств с одного счета на другой
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/accounts/transfer </code>
    /// </remarks>
    /// <returns>MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="201">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь неавторизован</response>
    /// <response code="404">Исходный счет не существует или не принадлежит текущему пользователю</response>
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
    /// <response code="401">Пользователь неавторизован</response>
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
        var query = new GetTransactionsForAccount.Query(GetUserGuid(), accountId, fromDate, toDate);
        var transactionList = await mediator.Send(query);
        return Success(StatusCodes.Status200OK, transactionList);
    }

    /// <summary>
    /// Возвращает информацию о транзакции
    /// </summary>
    /// <remarks>
    /// <code>
    /// GET {{address}}/api/accounts/transactions/{transactionId:guid} </code>
    /// </remarks>
    /// <returns> MbResult&lt;TransactionDto&gt;</returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь неавторизован</response>
    /// <response code="404">Счет не существует или не принадлежит текущему пользователю</response>
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

