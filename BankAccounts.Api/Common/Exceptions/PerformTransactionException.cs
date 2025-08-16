namespace BankAccounts.Api.Common.Exceptions
{
    public class PerformTransactionException(string message, Exception ex) : Exception(message, ex);
}
