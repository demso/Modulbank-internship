using BankAccounts.Api.Common;
using BankAccounts.Api.Common.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace BankAccounts.Api.Middleware;

/// <summary>
/// Middleware для перехвата исключений
/// </summary>
public class CustomExceptionHandlerMiddleware(RequestDelegate next)
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
        }
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        Console.WriteLine(exception.StackTrace);

        if (result == string.Empty)
            result = JsonSerializer.Serialize(MbResult.Failure((int)code, $"[{exception.GetType().Name}] {exception.Message}"));

        return context.Response.WriteAsync(result);
    }
}

