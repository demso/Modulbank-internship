namespace BankAccounts.Api.Common.Exceptions;

public class ConcurrencyException(string message, Exception innerException) 
    : InvalidOperationException(message, innerException)
{ }