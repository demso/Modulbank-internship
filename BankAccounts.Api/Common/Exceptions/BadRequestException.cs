namespace BankAccounts.Api.Common.Exceptions
{
    /// <summary>
    /// Исключение вбрасываемое в случае неверных входных данных, но их нецелесообразно или невозможно проверить в проверяющем
    /// </summary>
    /// <param name="details">Описание ошибки</param>
    public class BadRequestException(string details = "") : Exception("Неверный запрос. " + details);
}