using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Receiver.Handlers.Antifraud
{
    /// <summary>
    /// Интерфейс обработчика Antifraud сообщений
    /// </summary>
    public interface IAntifraudMessageHandler
    {
        /// <summary>
        /// Обработка сообщений о блокировке/разблокировке
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="ea"></param>
        Task ProcessAntifraudMessage(IChannel channel, BasicDeliverEventArgs ea);
    }
}