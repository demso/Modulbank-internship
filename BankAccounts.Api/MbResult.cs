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

    public static MbResult<T> Failure(int statusCode, string? mbError)
        => new(statusCode, mbError);
}

public record MbResult : MbResult<object?>
{
    private MbResult(int statusCode, object? value) : base(statusCode, value) { }

    public static MbResult Success(object? value) => new(StatusCodes.Status200OK, value);
}
