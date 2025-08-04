using BankAccounts.Api.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace BankAccounts.Api.Features.Shared;
/// <summary>
/// Вспомогательные методы для возврата результатов запросов с использованием  MbResult
/// </summary>
public class CustomControllerBase : ControllerBase
{
    protected Guid GetUserGuid()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    }

    protected MbResult Success(int statusCode)
    {
        HttpContext.Response.StatusCode = statusCode;
        return MbResult.Success(statusCode);
    }

    protected MbResult<T> Success<T>(int statusCode, T value)
    {
        HttpContext.Response.StatusCode = statusCode;
        return MbResult<T>.Success(statusCode, value);
    }
}