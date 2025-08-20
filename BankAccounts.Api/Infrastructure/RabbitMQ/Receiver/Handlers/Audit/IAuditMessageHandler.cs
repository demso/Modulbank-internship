using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Receiver.Handlers.Audit
{
    /// <summary>
    /// Интерфейс обработчика Audit сообщений
    /// </summary>
    public interface IAuditMessageHandler
    {
        /// <summary>
        /// Обработка всех полученных сообщений 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="ea"></param>
        /// <returns></returns>
        Task ProcessAuditMessage(IChannel channel, BasicDeliverEventArgs ea);
    }
}