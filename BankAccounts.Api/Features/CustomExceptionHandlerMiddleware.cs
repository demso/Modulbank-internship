using BankAccounts.Api.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace BankAccounts.Api.Features;

public class CustomExceptionHandlerMiddleware(RequestDelegate next)
{
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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;
        switch (exception)
        {
            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(MbResult<object?>.Failure((int)code, validationException.Errors.First().ToString()));
                break;
            case AccountNotFoundException:
                code = HttpStatusCode.NotFound;
                break;
            case NotFoundException:
                code = HttpStatusCode.NotFound;
                break;
            case not null:
                code = HttpStatusCode.BadRequest;
                break;
        }
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        if (result == string.Empty)
        {
            result = JsonSerializer.Serialize(MbResult<object?>.Failure((int)code, exception?.Message));
        }

        return context.Response.WriteAsync(result);
    }
}

