namespace BankAccounts.Api.Common.Exceptions;

/// <summary>
/// Исключение, возникающее при конфликте версий данных при транфере средств.
/// </summary>
/// <param name="message">Сообщение</param>
/// <param name="innerException">Внутреннее исключение</param>
public class ConcurrencyException(string message, Exception innerException) 
    : InvalidOperationException(message, innerException);