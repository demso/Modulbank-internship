// ReSharper disable UnusedAutoPropertyAccessor.Global
// Назначение методов понятно из названия
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BankAccounts.Api.Common;

/// <summary>
/// Класс для возврата данных о результатах операции клиенту.
/// </summary>
/// <typeparam name="T">Тип данных аозвращаемого значения</typeparam>
public record MbResult<T>
{
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public T? Value { get; }
    public string? MbError { get; }

    private protected MbResult(int statusCode, T value)
    {
        IsSuccess = true;
        StatusCode = statusCode;
        Value = value;
    }

    private MbResult(int statusCode, string? mbError)
    {
        IsSuccess = false;
        MbError = mbError;
        StatusCode = statusCode;
    }

    public static MbResult<T> Success(int code, T value) => new(code, value);

    public static MbResult<T> Failure(int statusCode, string? mbError)
        => new(statusCode, mbError);
}

/// <summary>
/// Используем этот класс если запрос не возвращает ничего кроме мтатус-кода.
/// </summary>
public record MbResult : MbResult<object?>
{
    private MbResult(int statusCode, object? value) : base(statusCode, value) { }

    public static MbResult Success(int code) => new(code, null);
}
