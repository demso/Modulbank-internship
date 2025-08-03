using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankAccounts.Api;
// вспомогательные методы для возврата результатов запросов с использованием  MbResult
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

    protected MbResult<T> Failure<T>(int statusCode, T message)
    {
        HttpContext.Response.StatusCode = statusCode;
        return MbResult<T>.Failure(statusCode, message?.ToString());
    }
}