using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter;
using BankAccounts.Api.Infrastructure.UserBlacklist;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BankAccounts.Api.Infrastructure.RabbitMQ
{
    /// <summary>
    /// Обработчик входящих сообщений RabbitMQ
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="scopeFactory"></param>
    /// <param name="configuration"></param>
    public class Receiver(ILogger<Receiver> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration) 
        : IHostedService
    {
        private ConnectionFactory _factory = null!;
        private IConnection _connection = null!;
        private IChannel _channel = null!;
        private readonly string _exchangeName = configuration["RabbitMQ:ExchangeName"]!;
        private const string AuditQueueName = "account.audit";
        private const string AntifraudQueueName = "account.antifraud";
        private const int MajorMessageVersion = 1;

        private async Task Init()
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Hostname"]!, 
                UserName = configuration["RabbitMQ:Username"]!, 
                Password = configuration["RabbitMQ:Password"]!
            };
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            
            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );
            
            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic);
            await SetupQueues();
            
            AsyncEventingBasicConsumer auditConsumer = new(_channel);
            auditConsumer.ReceivedAsync += ProcessAuditMessage;
            
            AsyncEventingBasicConsumer antifraudConsumer = new(_channel);
            antifraudConsumer.ReceivedAsync += ProcessAntifraudMessage;

            await _channel.BasicConsumeAsync(AntifraudQueueName, autoAck: false, consumer: antifraudConsumer);
            await _channel.BasicConsumeAsync(AuditQueueName, autoAck: false, consumer: auditConsumer);
        }

        /// <summary>
        /// Обработка всех полученных сообщений 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ea"></param>
        /// <returns></returns>
        public async Task ProcessAuditMessage(object model, BasicDeliverEventArgs ea)
        {
            const string handler = "None";
            try
            { 
                using IServiceScope scope = scopeFactory.CreateScope();
                IBankAccountsDbContext dbContext = scope.ServiceProvider.GetRequiredService<IBankAccountsDbContext>();
                byte[] body = ea.Body.ToArray();
                string message = BytesToString(body);
                JsonDocument document = JsonDocument.Parse(message);

                string? reason = ValidateMessage(ea.BasicProperties, document, 
                    out EventType? eventType, 
                    out Guid? messageId);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract На всякий случай
                if (reason != null)
                {
                    LogDeadLetter(document, eventType, reason, handler);
                    await AddToDeadLetters(dbContext, messageId, document.RootElement.GetRawText(), eventType,
                        DateTime.UtcNow, handler,
                        reason);
                    await Nack(deliveryTag: ea.DeliveryTag, requeue: false);
                    return;
                }
                
                bool alreadyAdded = await dbContext.InboxConsumed
                    .AnyAsync(ic => ic.MessageId == messageId!.Value, CancellationToken.None);

                if (alreadyAdded)
                {
                    // Сообщение уже обработано ранее, просто подтверждаем его.
                    logger.LogWarning("Message with MessageId {MessageId} already added ({handler}).", messageId, handler);
                    await Ack(deliveryTag: ea.DeliveryTag);
                    return;
                }

                await AddToInbox(dbContext, messageId!.Value, eventType!.Value, DateTime.UtcNow, handler);
                
                await Ack(deliveryTag: ea.DeliveryTag);
                
                LogSuccess(document, eventType.Value, messageId.Value, GetTimestamp(ea), handler);
            }
            catch (Exception ex)
            {
                await Nack(deliveryTag: ea.DeliveryTag, requeue: !ea.Redelivered);
                logger.LogInformation("Error while processing message, is requeued: {Requeue} ({handler}). {Message} "
                    , handler, !ea.Redelivered, ex.Message);
                await Task.Delay(1000);
                throw;
            }
        }

        /// <summary>
        /// Обработка сообщений о блокировке/разблокировке
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ea"></param>
        public async Task ProcessAntifraudMessage(object model, BasicDeliverEventArgs ea)
        {
            const string handler = "Antifraud";
            try
            { 
                using IServiceScope scope = scopeFactory.CreateScope();
                IBankAccountsDbContext dbContext = scope.ServiceProvider.GetRequiredService<IBankAccountsDbContext>();
                IUserBlacklistService blacklist = scope.ServiceProvider.GetRequiredService<IUserBlacklistService>();
               
                (EventType, Guid, JsonDocument)? result = await CheckDeadLetter(dbContext, ea, handler);

                if (result is null)
                {
                    await Nack(ea.DeliveryTag, false);
                    return;
                }

                JsonDocument document = result.Value.Item3;
                EventType eventType = result.Value.Item1;
                Guid messageId =  result.Value.Item2;
                
                List<InboxConsumedEntity> entities = await dbContext.InboxConsumed
                    .Where(ic => ic.MessageId == messageId).AsNoTracking()
                    .ToListAsync(CancellationToken.None);
                
                int addedOrProcessed = CheckAlreadyAdded(entities);
                
                bool isProcessed = addedOrProcessed == 2;
                bool isAdded = addedOrProcessed == 1;

                if (isProcessed)
                {
                    // Сообщение уже обработано ранее, просто подтверждаем его.
                    logger.LogWarning("Message with MessageId {MessageId} already processed ({Handler}).", messageId, handler);
                    await Nack(deliveryTag: ea.DeliveryTag, false);
                    return;
                }

                Guid clientId = document.RootElement.GetProperty("clientId").GetGuid();
                
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault Нужно обработать только 2 случая
                switch (eventType)
                {
                    case EventType.ClientBlocked:
                        await ClientBlockedReceived(blacklist, clientId);
                        break;
                    case EventType.ClientUnblocked:
                        await ClientUnblockedReceived(blacklist, clientId);
                        break;
                }
                
                if (isAdded)
                {
                    InboxConsumedEntity inboxEntry = entities[0];
                    inboxEntry.Handler = handler;
                    dbContext.InboxConsumed.Update(inboxEntry);
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                }
                else
                {
                    await AddToInbox(dbContext, messageId, eventType, DateTime.UtcNow, handler);
                }
               
                LogSuccess(document, eventType, messageId, GetTimestamp(ea), handler);

                await Ack(deliveryTag: ea.DeliveryTag);
            }
            catch (Exception ex)
            { 
                await Nack(deliveryTag: ea.DeliveryTag, requeue: !ea.Redelivered);
                logger.LogInformation("Error while processing message ({handler}), is requeued: {Requeue}. {Message}"
                    ,handler ,!ea.Redelivered, ex.Message);
                await Task.Delay(1000);
                throw;
            }
        }

        private async Task ClientBlockedReceived(IUserBlacklistService blacklist, Guid userId)
        {
            bool wasBlocked = await blacklist.AddToList(userId);
            if (wasBlocked)
                logger.LogInformation("{Message}",
                    "[CLIENT_BLOCKED] Client is already blocked with id "  + userId);
            else
                logger.LogInformation("{Message}",
                    "[CLIENT_BLOCK] Client blocked with id "  + userId);
        }

        private async Task ClientUnblockedReceived(IUserBlacklistService blacklist, Guid userId)
        {
            bool wasInList = await blacklist.RemoveFromList(userId);
            if (wasInList)
                logger.LogInformation("{Message}",
                "[CLIENT_UNBLOCKED] Client was not blocked with id " + userId);
            else
                logger.LogInformation("{Message}",
                    "[CLIENT_UNBLOCK] Client unblocked with id " + userId);
        }


        // ReSharper disable once ReturnTypeCanBeNotNullable Может быть null
        private async Task<(EventType, Guid, JsonDocument)?> CheckDeadLetter(IBankAccountsDbContext dbContext, BasicDeliverEventArgs ea, string handler)
        {
            byte[] body = ea.Body.ToArray();
            string message = BytesToString(body);
            JsonDocument document = JsonDocument.Parse(message);

            string? reason = ValidateMessage(ea.BasicProperties, document, 
                out EventType? eventType, 
                out Guid? messageId);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
            // ReSharper disable once InvertIf Предлагает непонятный код
            if (reason != null)
            {
                LogDeadLetter(document, eventType, reason, handler);
                await AddToDeadLetters(dbContext, messageId, document.RootElement.GetRawText(), eventType,
                    DateTime.UtcNow, handler,
                    reason);
                return null;
            }
                
            return (eventType!.Value, messageId!.Value, document);
        }

        private static DateTime? GetTimestamp(BasicDeliverEventArgs ea)
        {
            long propTimestamp = ea.BasicProperties.Timestamp.UnixTime;
            DateTime? timestamp = propTimestamp == 0 ? null : DateTime.FromBinary(propTimestamp);
            return timestamp;
        }

        private async Task Ack(ulong deliveryTag)
        {
            await _channel.BasicAckAsync(deliveryTag, multiple: false);
        }

        private async Task Nack(ulong deliveryTag, bool requeue)
        {
            await _channel.BasicNackAsync(deliveryTag: deliveryTag, multiple: false, requeue: requeue);
        }

        private static int CheckAlreadyAdded(List<InboxConsumedEntity> entities)
        {
            int result = 0;
            
            bool alreadyAdded = entities.Count != 0;

            if (!alreadyAdded)
            {
                return result;
            }

            result = 1;
            bool alreadyProcessed = entities[0].Handler != "None";
            if (alreadyProcessed)
            {
                result = 2;
            }

            return result;
        }

        private static string ValidateMessage(IReadOnlyBasicProperties properties, JsonDocument document, 
            out EventType? eventType, out Guid? messageId)
        {
            eventType = null;
            messageId = null;
            
            bool hasHeaders = false;
                
            string headerCausationId = null!;
            string headerCorrelationId = null!;

            string version = null!;
            string bodyCausationId = null!;
            string bodyCorrelationId = null!;

            string? reason = null!;
            
            try
            {
                IDictionary<string, object?> headers = properties.Headers!;
                hasHeaders = headers.Count != 0;
                
                eventType = Enum.Parse<EventType>(BytesToString(headers["type"]!));
                messageId = Guid.Parse((ReadOnlySpan<char>)properties.MessageId);
                
                headerCausationId = BytesToString(headers["x-causation-id"]!);
                headerCorrelationId = BytesToString(headers["x-correlation-id"]!);

                version = document.RootElement.GetProperty("meta").GetProperty("version").GetString()!;
                bodyCausationId =
                    document.RootElement.GetProperty("meta").GetProperty("causationId").GetString()!;
                bodyCorrelationId = document.RootElement.GetProperty("meta").GetProperty("correlationId")
                    .GetString()!;
                
                if (headerCausationId != bodyCausationId)
                {
                    reason = "Causation id mismatch";
                }
                else if (headerCorrelationId != bodyCorrelationId)
                {
                    reason = "Correlation id mismatch";
                }
                else if (int.Parse(version.Substring(1, 1)) > MajorMessageVersion) // проверка на соответствие версии
                {
                    reason = "Message version not supported";
                } 
                else if (eventType is EventType.ClientBlocked or EventType.ClientUnblocked)
                {
                    try
                    {
                        document.RootElement.GetProperty("clientId").GetGuid();
                    }
                    catch (Exception)
                    {
                        reason = "No client id found";
                    }
                }
            }
            catch (Exception)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                if (reason is null)
                {
                    if (!hasHeaders)
                        reason = "No headers";
                    else if (eventType is null)
                        reason = "No event type";
                    else if (messageId is null)
                        reason = "No message id";
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                    else if (headerCausationId is null)
                        reason = "No header causation";
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                    else if (headerCorrelationId is null)
                        reason = "No header correlation";
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                    else if (version is null)
                        reason = "No version";
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                    else if (bodyCausationId is null)
                        reason = "No body causation";
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                    else if (bodyCorrelationId is null)
                        reason = "No body correlation";
                    else 
                        reason = "Unknown reason";
                }
            }
            
            return reason;
        }

        private static string BytesToString(object bytes)
        {
            return Encoding.UTF8.GetString((byte[]) bytes);
        }
        
        private void LogSuccess(JsonDocument document, EventType type, Guid? messageId, DateTime? timestamp, string handler)
        {
            string? id = null;
            string? correlationId = null;
            string? ownerId = null;
            
            try
            {
                JsonElement root = document.RootElement;
                    
                id = root.GetProperty("eventId").GetString();
                correlationId = root.GetProperty("meta").GetProperty("correlationId").GetString();
                ownerId = root.GetProperty("ownerId").GetString();
            }
            catch (Exception) { /* ignored */ }
            
            TimeSpan? latency = timestamp == null ? null : DateTime.UtcNow - timestamp;
                    
            logger.LogInformation("Successfully consumed event: id = {id}, ownerId = {owner}, type = {type}, " +
                                  "correlationId = {correlationId}, messageId = {MessageId},latency = {latency}, handler = {hanler}",
                id, ownerId, type.ToString(), correlationId, messageId, latency, handler); 
        }
        
        private void LogDeadLetter(JsonDocument document, EventType? type, string reason, string handler)
        {
            string? id = null;
            string? correlationId = null;

            try
            {
                JsonElement root = document.RootElement;
                    
                id = root.GetProperty("eventId").GetString();
                correlationId = root.GetProperty("meta").GetProperty("correlationId").GetString();
            }
            catch (Exception) {/* ignored */ }
                    
            logger.LogInformation("Consumed \"dead letter\" event: id = {id}, type = {type}, " +
                                  "correlationId = {correlationId}, reason = {reason}, handler = {handler}", id, 
                type.ToString(), correlationId, reason, handler); // eventId, type, correlationId, retry, latency
        }

        private static async Task AddToInbox(IBankAccountsDbContext dbContext, Guid messageId, EventType eventType, 
            DateTime processedAt, string handler)
        {
            InboxConsumedEntity inboxConsumed = new()
            {
                MessageId = messageId,
                EventType = eventType, 
                ProcessedAt = processedAt, 
                Handler = handler
            };
        
            await dbContext.InboxConsumed.AddAsync(inboxConsumed);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        
        private async Task AddToDeadLetters(IBankAccountsDbContext dbContext, Guid? messageId, string message,
            EventType? eventType, DateTime receivedAt, string handler, string error)
        {
            if (dbContext.DeadLetters.Where(e => e.MessageId == messageId).ToList().Count != 0)
            {
                logger.LogWarning("Dead letter with messageId = {messageId} is already added to inbox_dead_letters",
                    messageId);
                return;
            }
            DeadLetterEntity deadLetter = new()
            {
                MessageId = messageId ?? Guid.NewGuid(),
                ReceivedAt = receivedAt,
                Handler = handler,
                Payload = message,
                EventType = eventType,
                Error = error
            };
            
            _ = (await dbContext.DeadLetters.AddAsync(deadLetter)).Entity;
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        
        /// <summary>
        /// Создаст очереди принимающие события по типам для наглядного представления отправленных сообщений
        /// </summary>
        private async Task SetupQueues()
        {
            await _channel.QueueDeclareAsync(
                queue: AntifraudQueueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false,
                arguments: null);
            
            await _channel.QueueBindAsync(AntifraudQueueName, _exchangeName, "client.#");

            await _channel.QueueDeclareAsync(
                queue: AuditQueueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false,
                arguments: null
            );
            
            await _channel.QueueBindAsync(AuditQueueName, _exchangeName, "#");
            
            foreach (EventType type in Enum.GetValues<EventType>())
            {
                if (type is EventType.ClientBlocked or EventType.ClientUnblocked)
                    continue;
                
                await _channel.QueueDeclareAsync(queue: $"test_{type.ToString()}", durable: true, false, false);
                await _channel.QueueBindAsync($"test_{type.ToString()}", _exchangeName, Event.GetRoute(type));
            }
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Init();
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        { 
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Null в исключительных случаях
            if (_channel != null)
                await _channel.DisposeAsync();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Null в исключительных случаях
            if (_connection != null)
                await _connection.DisposeAsync();
        }
    }
}