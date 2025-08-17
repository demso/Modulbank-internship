namespace BankAccounts.Api.Common.Exceptions
{
    /// <summary>
    /// Выбрасывается, когда пользователь заблокирован и пытается провести транзакции по счету
    /// </summary>
    /// <param name="id"></param>
    public class UserInBlockListException(Guid id) : Exception(id.ToString());
}
