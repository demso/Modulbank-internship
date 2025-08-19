using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Api.Infrastructure.UserBlacklist;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Receiver.Handlers.Antifraud
{
    /// <summary>
    /// Обработчик Antifraud сообщений
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="blacklist"></param>
    /// <param name="logger"></param>
    public class AntifraudMessageHandler(IBankAccountsDbContext dbContext, IUserBlacklistService blacklist,
        ILogger<AntifraudMessageHandler> logger) : AbstractMessageHandler<AntifraudMessageHandler>(logger, "Antifraud"),
        IAntifraudMessageHandler
    {
        private readonly ILogger<AntifraudMessageHandler> _logger = logger;

        private const string Handler = "Antifraud";
        /// <inheritdoc />
        public async Task ProcessAntifraudMessage(IChannel channel, BasicDeliverEventArgs ea)
        {
            try
            {
                (EventType, Guid, JsonDocument)? result 
                    = await CheckAndSaveIfDeadLetterAck(channel, dbContext, ea, Handler);

                if (result is null) // Сообщение некорректно
                    return;
                

                JsonDocument document = result.Value.Item3;
                EventType eventType = result.Value.Item1;
                Guid messageId = result.Value.Item2;

                InboxConsumedEntity? entity = await GetEntity(messageId);

                int addedOrProcessed = CheckAlreadyAdded(entity);

                bool isProcessed = addedOrProcessed == 2;
                bool isAdded = addedOrProcessed == 1;

                if (isProcessed)
                {
                    await MessageProcessedAck(messageId, channel, ea.DeliveryTag);
                    return;
                }

                Guid clientId = document.RootElement.GetProperty("clientId").GetGuid();

                await ProcessMessage(eventType, clientId);

                if (isAdded)
                    await Handled(entity!);
                else
                    await AddHandledToInbox(dbContext, messageId, eventType, DateTime.UtcNow, Handler);

                LogSuccess(document, eventType, messageId, GetTimestamp(ea), Handler);

                await Ack(channel, deliveryTag: ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                await ErrorNack(channel, ea, ex);
                throw;
            }
        }

        private async Task Handled(InboxConsumedEntity entity)
        {
            entity.Handler = Handler;
            dbContext.InboxConsumed.Update(entity);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        private async Task MessageProcessedAck(Guid messageId, IChannel channel, ulong deliveryTag)
        {
            // Сообщение уже обработано ранее, просто подтверждаем его.
            LogMessageProcessed(messageId, Handler);
            await Ack(channel, deliveryTag: deliveryTag);
        }

        private async Task<InboxConsumedEntity?> GetEntity(Guid messageId)
        {
            List<InboxConsumedEntity> entities = await dbContext.InboxConsumed
                .Where(ic => ic.MessageId == messageId).AsNoTracking()
                .ToListAsync(CancellationToken.None);
            return entities.Count > 0 ? entities[0] : null;
        }

        private async Task ProcessMessage(EventType eventType, Guid userId)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault Нужно обработать только 2 случа
            switch (eventType)
            {
                case EventType.ClientBlocked:
                    await ClientBlockedReceived(userId);
                    break;
                case EventType.ClientUnblocked:
                    await ClientUnblockedReceived(userId);
                    break;
            }
        }

        private async Task ClientBlockedReceived(Guid userId)
        {
            bool wasBlocked = await blacklist.AddToList(userId);
            if (wasBlocked)
                _logger.LogInformation("{Message}",
                    "[CLIENT_BLOCKED] Client is already blocked with id " + userId);
            else
                _logger.LogInformation("{Message}",
                    "[CLIENT_BLOCK] Client blocked with id " + userId);
        }

        private async Task ClientUnblockedReceived(Guid userId)
        {
            bool wasInList = await blacklist.RemoveFromList(userId);
            if (wasInList)
                _logger.LogInformation("{Message}",
                    "[CLIENT_UNBLOCKED] Client was not blocked with id " + userId);
            else
                _logger.LogInformation("{Message}",
                    "[CLIENT_UNBLOCK] Client unblocked with id " + userId);
        }
    }
}