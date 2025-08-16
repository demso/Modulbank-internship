namespace BankAccounts.Api.Common.Exceptions
{
    public class AccrueInterestException(string message, Exception innerException) : Exception(message, innerException);
}
