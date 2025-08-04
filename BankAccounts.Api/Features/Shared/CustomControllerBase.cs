using BankAccounts.Api.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankAccounts.Api.Features.Shared;
/// <summary>
/// Вспомогательные методы для возврата результатов запросов с использованием  MbResult
/// </summary>
public class CustomControllerBase : ControllerBase
{
    /// <summary>
    /// Возвращает Guid пользователя
    /// </summary>
    /// <returns></returns>
    protected Guid GetUserGuid()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    }

    /// <summary>
    /// Возвращает MbResult с IsSuccess = true и заданный кодом
    /// </summary>
    /// <param name="statusCode">Http статус код</param>
    /// <returns>MbResult</returns>
    protected MbResult Success(int statusCode)
    {
        HttpContext.Response.StatusCode = statusCode;
        return MbResult.Success(statusCode);
    }

    /// <summary>
    /// Возвращает MbResult с IsSuccess = true, заданный кодом и значением
    /// </summary>
    /// <param name="statusCode">Http статус код</param>
    /// <param name="value">Результат выполнения операции</param>
    /// <returns>MbResult</returns>
    protected MbResult<T> Success<T>(int statusCode, T value)
    {
        HttpContext.Response.StatusCode = statusCode;
        return MbResult<T>.Success(statusCode, value);
    }
}