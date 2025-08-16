namespace BankAccounts.Api.Common.Exceptions
{
    public class CreateAccountException(string msg, Exception ex) : Exception(msg, ex);
}
