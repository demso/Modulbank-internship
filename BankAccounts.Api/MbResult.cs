namespace BankAccounts.Api;

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

    public static MbResult<T> Success(T value) => new(StatusCodes.Status200OK, value);

    public static MbResult<T> NoContent() => new(StatusCodes.Status204NoContent, null);

    public static MbResult<T> Created(T value) => new(StatusCodes.Status201Created, value);;

    public static MbResult<T> Failure(int statusCode, string? mbError)
        => new(statusCode, mbError);

    public static MbResult<T> NotFound(string message = "Не найдено")
        => Failure(StatusCodes.Status404NotFound, message);

    public static MbResult<T> Unauthorized(string message = "Доступ запрещён")
        => Failure(StatusCodes.Status401Unauthorized, message);

    public static MbResult<T> BadRequest(string message = "Некорректный запрос")
        => Failure(StatusCodes.Status400BadRequest, message);

    public static MbResult<T> InternalError(string message = "Внутренняя ошибка сервера")
        => Failure(StatusCodes.Status500InternalServerError, message);
}

public record MbResult : MbResult<object?>
{
    private MbResult(int statusCode, object? value) : base(statusCode, value) { }

    private MbResult(int statusCode, string? mbError) : base(statusCode, mbError) { }

    public new static MbResult Success(int code, object? value) => new(code, value);

    public new static MbResult Success(object? value) => new(StatusCodes.Status200OK, value);

    public new static MbResult NoContent() => new(StatusCodes.Status204NoContent, null);

    public new static MbResult Created(object? value) => new(StatusCodes.Status201Created, value);

    public new static MbResult Failure(string? mbError, int statusCode = StatusCodes.Status400BadRequest)
        => new(statusCode, mbError);

}
