namespace BankAccounts.Api.Common.Exceptions
{
    /// <summary>
    /// Исключение вызываемое при ошибке начисления процентов по счету
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public class AccrueInterestException(string message, Exception innerException) : Exception(message, innerException);
}
