using BankAccounts.Api.Features.Shared.UserBlacklist;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter;
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
    public class Receiver(ILogger<Receiver> logger, IServiceScopeFactory scopeFactory, IUserBlacklistService blacklist, IConfiguration configuration) 
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
        /// Обработка сообщений о блокировке/разблокировке
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ea"></param>
        public async Task ProcessAntifraudMessage(object model, BasicDeliverEventArgs ea)
        {
            (EventType, JsonDocument)? result = await ProcessAuditMessage(model, ea);
            if (result is null)
                return;
            
            EventType eventType = result.Value.Item1;
            JsonDocument jsonDocument = result.Value.Item2;
            Guid clientId = jsonDocument.RootElement.GetProperty("ClientId").GetGuid();
            
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault Нужно обработать только 2 случая
            switch (eventType)
            {
                case EventType.ClientBlocked:
                    ClientBlockedReceived(clientId);
                    break;
                case EventType.ClientUnblocked:
                    ClientUnblockedReceived(clientId);
                    break;
            }
        }

        private void ClientBlockedReceived(Guid userId)
        {
            blacklist.AddToList(userId);
        }

        private void ClientUnblockedReceived(Guid userId)
        {
            blacklist.RemoveFromList(userId);
        }

        /// <summary>
        /// Обработка всех полученных сообщений 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ea"></param>
        /// <returns></returns>
        public async Task<(EventType, JsonDocument)?> ProcessAuditMessage(object model, BasicDeliverEventArgs ea)
        {
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

                if (reason != null)
                {
                    LogDeadLetter(document, eventType, reason);
                    await AddToDeadLetters(dbContext, messageId, document.RootElement.GetRawText(), eventType,
                        DateTime.UtcNow, "Receiver",
                        reason);
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    return null;
                }

                await AddToInbox(dbContext, messageId!.Value, eventType!.Value, DateTime.UtcNow, "Receiver");
                
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                
                long propTimestamp = ea.BasicProperties.Timestamp.UnixTime;
                DateTime? timestamp = propTimestamp == 0 ? null : DateTime.FromBinary(propTimestamp);
                LogSuccess(document, eventType.Value, timestamp);
                
                return (eventType.Value,  document);
            }
            catch (Exception)
            {
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                logger.LogInformation("Error while processing message, requeued.");
                throw;
            }
        }

        // ReSharper disable once ReturnTypeCanBeNotNullable Может быть null
        private static string? ValidateMessage(IReadOnlyBasicProperties properties, JsonDocument document, out EventType? eventType, out Guid? messageId)
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
                
                headerCausationId = BytesToString(headers["x-causation-id"]!);
                headerCorrelationId = BytesToString(headers["x-correlation-id"]!);

                version = document.RootElement.GetProperty("Metadata").GetProperty("Version").GetString()!;
                bodyCausationId =
                    document.RootElement.GetProperty("Metadata").GetProperty("CausationId").GetString()!;
                bodyCorrelationId = document.RootElement.GetProperty("Metadata").GetProperty("CorrelationId")
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
                        document.RootElement.GetProperty("ClientId").GetGuid();
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
        
        private void LogSuccess(JsonDocument document, EventType type, DateTime? timestamp)
        {
            string? id = null;
            string? correlationId = null;
            
            try
            {
                JsonElement root = document.RootElement;
                    
                id = root.GetProperty("EventId").GetString();
                correlationId = root.GetProperty("Metadata").GetProperty("CorrelationId").GetString();
            }
            catch (Exception) { /* ignored */ }
            
            TimeSpan? latency = timestamp == null ? null : DateTime.UtcNow - timestamp;
                    
            logger.LogInformation("Successfully consumed event: id = {id}, type = {type}, " +
                                  "correlationId = {correlationId}, latency = {latency}", id, 
                type.ToString(), correlationId, latency); // eventId, type, correlationId, retry, latency
        }
        
        private void LogDeadLetter(JsonDocument document, EventType? type, string reason)
        {
            string? id = null;
            string? correlationId = null;

            try
            {
                JsonElement root = document.RootElement;
                    
                id = root.GetProperty("EventId").GetString();
                correlationId = root.GetProperty("Metadata").GetProperty("CorrelationId").GetString();
            }
            catch (Exception) {/* ignored */ }
                    
            logger.LogInformation("Consumed \"dead letter\" event: id = {id}, type = {type}, " +
                                  "correlationId = {correlationId}, reason = {reason}", id, 
                type.ToString(), correlationId, reason); // eventId, type, correlationId, retry, latency
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
        
        private async Task AddToDeadLetters(IBankAccountsDbContext dbContext, Guid? messageId, string message, EventType? eventType,
            DateTime receivedAt, string handler, string error)
        {
            try {
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
                
            } catch (Exception e) { logger.LogError("{Message}",
                "Failed to add message to dead letters.\n" + e.Message + "\n" + e.StackTrace); }
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