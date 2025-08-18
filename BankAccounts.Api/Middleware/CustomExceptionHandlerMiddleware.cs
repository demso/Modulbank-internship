using BankAccounts.Api.Common;
using BankAccounts.Api.Common.Exceptions;
using FluentValidation;
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
    // ReSharper disable once UnusedMember.Global Метод используется
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
    /// Обработка исключений. Возвращает данные об ошибке с помощью <see cref="MbResult"/> (записывает ошибку в поле MbError).
    /// </summary>
    // ReSharper disable once MemberCanBeMadeStatic.Local Метод используется
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode code = HttpStatusCode.BadRequest;
        string message = string.Empty;
        
        ProcessException(exception, ref code, ref message);
        
        if (message == string.Empty)
            message = $"[{exception.GetType().Name}] {exception.Message}";

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(MbResult.Failure((int)code, message)));
    }

    /// <summary>
    /// Обработка исключения
    /// </summary>
    private void ProcessException(Exception exception, ref HttpStatusCode code, ref string message)
    {
        Exception? realException = ExceptionDiver(exception);
        
        switch (exception)
        {
            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                message = validationException.Errors.First().ToString();
                break;
            case NotFoundException:
                code = HttpStatusCode.NotFound;
                break;
            case TransferException:
                if (realException is not null && CheckIfDbConcurrencyAccessException(realException))
                    code = HttpStatusCode.Conflict;
                break;
        }

        switch (realException)
        {
            case UserInBlockListException:
                code = HttpStatusCode.Conflict;
                message = "Счета пользователя заблокированы";
                break;
            case BadRequestException:
                code = HttpStatusCode.BadRequest;
                message = realException.Message;
                break;
        }
    }
    
    private static bool CheckIfDbConcurrencyAccessException(Exception ex)
    {
        return ex.Message.Contains("40001: could not serialize");
    }

    /// <summary>
    /// Записывает в лог каждое исключение и возвращает самое вложенное.
    /// </summary>
    /// <param name="exception">Исключение на обработку</param>
    /// <returns>Самое вложенное исключение</returns>
    private Exception? ExceptionDiver(Exception exception)
    {
        Exception? exceptionDiver = exception.InnerException;
        Exception? realException = exceptionDiver;
        int count = 0;
        string message = string.Empty;

        AddToMessage(exception, count, ref message);
        
        while (exceptionDiver != null)
        {
            count++;
            
            // Вывести stackTrace только у самого вложенного исключения
            if (exceptionDiver.InnerException != null)
                AddToMessage(exceptionDiver, count, ref message, false);
            else
                AddToMessage(exceptionDiver, count, ref message);
            
            realException = exceptionDiver;
            exceptionDiver = exceptionDiver.InnerException;
        }
        
        logger.LogError("{Message}", message);
        
        return realException;
    }

    private static void AddToMessage(Exception ex, int count, ref string message, bool logStackTrace = true)
    {
        string start = count == 0 ? "" : "\n";
        message += $"{start}({count}) {ex.GetType().Name}: {ex.Message} " + 
            (logStackTrace ? $"\n{ex.StackTrace}" : string.Empty);
    }
}

