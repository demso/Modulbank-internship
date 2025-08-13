using BankAccounts.Api.Features.Transactions.Commands.PerformTransfer;

namespace BankAccounts.Api.Common.Exceptions
{
    /// <summary>
    /// Исключение выбрасываемое в <see cref="PerformTransferHandler"/> при ошибке трансфера
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="ex">Исключение</param>
    public class TransferException(string message, Exception ex) : Exception(message, ex)
    {
        
    }
}