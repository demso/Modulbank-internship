namespace BankAccounts.Api.Common.Exceptions
{
    /// <summary>
    /// Исключение, вызываемое в случае неудачного проведения трансфера
    /// </summary>
    /// <param name="message"></param>
    /// <param name="ex"></param>
    public class PerformTransactionException(string message, Exception ex) : Exception(message, ex);
}
