using BankAccounts.Api.Common;
using BankAccounts.Api.Common.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace BankAccounts.Api.Middleware;

/// <summary>
/// Middleware для перехвата исключений
/// </summary>
public class CustomExceptionHandlerMiddleware(ILogger<CustomExceptionHandlerMiddleware> logger, RequestDelegate next)
{
    /// <summary>
    /// Встраивание в pipeline
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Обработка исключений. Возваращает данные об ошибке с помощью MbResult (записывает ошибку в поле MbError).
    /// </summary>
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.BadRequest;
        var result = string.Empty;
        switch (exception)
        {
            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(MbResult.Failure((int)code, validationException.Errors.First().ToString()));
                break;
            case AccountNotFoundException:
            case NotFoundException:
                code = HttpStatusCode.NotFound;
                break;

            case DbUpdateException:
                result = JsonSerializer.Serialize(MbResult.Failure((int)code, $"[{exception.GetType().Name}] {exception.Message} \n {exception?.InnerException?.Message} \n {exception?.InnerException?.StackTrace}"));
                break;
        }
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        logger.LogCritical($"\n\n\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception.GetType().Name}: {exception.Message}\n\n\n");
        logger.LogCritical(exception.StackTrace);
        logger.LogCritical($"\n\n\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception?.InnerException?.GetType().Name}: {exception?.InnerException?.Message}\n\n\n");
        logger.LogCritical(exception?.InnerException?.StackTrace);

        if (result == string.Empty)
            result = JsonSerializer.Serialize(MbResult.Failure((int)code, $"[{exception.GetType().Name}] {exception.Message}"));

        return context.Response.WriteAsync(result);
    }
}

