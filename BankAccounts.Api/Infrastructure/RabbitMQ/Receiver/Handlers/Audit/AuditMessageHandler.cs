using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Receiver.Handlers.Audit
{
    /// <summary>
    /// Обработчик Audit сообщений
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="logger"></param>
    public class AuditMessageHandler(IBankAccountsDbContext dbContext, ILogger<AuditMessageHandler> logger) 
        : AbstractMessageHandler<AuditMessageHandler>(logger, "None"), IAuditMessageHandler
    {
        /// <inheritdoc />
        public async Task ProcessAuditMessage(IChannel channel, BasicDeliverEventArgs ea)
        {
            const string handler = "None";
            try
            {
                (EventType, Guid, JsonDocument)? result 
                    = await CheckAndSaveIfDeadLetterAck(channel, dbContext, ea, handler);

                if (result is null) // Это мертвое письмо
                {
                    await Ack(channel, ea.DeliveryTag);
                    return;
                }

                JsonDocument document = result.Value.Item3;
                EventType eventType = result.Value.Item1;
                Guid messageId = result.Value.Item2;

                bool alreadyAdded = await dbContext.InboxConsumed
                    .AnyAsync(ic => ic.MessageId == messageId, CancellationToken.None);

                if (alreadyAdded)
                {
                    // Сообщение уже обработано ранее, просто подтверждаем его.
                    LogMessageProcessed(messageId, handler);
                    await Ack(channel, deliveryTag: ea.DeliveryTag);
                    return;
                }

                await AddHandledToInbox(dbContext, messageId, eventType, DateTime.UtcNow, handler);

                await Ack(channel,deliveryTag: ea.DeliveryTag);

                LogSuccess(document, eventType, messageId, GetTimestamp(ea), handler);
            }
            catch (Exception ex)
            {
                await ErrorNack(channel, ea, ex);
                throw;
            }
        }
    }
}