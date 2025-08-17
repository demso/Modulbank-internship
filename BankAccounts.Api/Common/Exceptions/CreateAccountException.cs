namespace BankAccounts.Api.Common.Exceptions
{
    /// <summary>
    /// Исключение, вызываемое в случае неудачного открытия счета.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="ex"></param>
    public class CreateAccountException(string msg, Exception ex) : Exception(msg, ex);
}
