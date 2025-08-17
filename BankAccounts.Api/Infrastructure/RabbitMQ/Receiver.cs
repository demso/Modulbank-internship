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
    public class Receiver(ILogger<Receiver> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration) : IHostedService
    {
        private ConnectionFactory factory;
        private IConnection connection;
        private IChannel channel;
        public const string ExchangeName = "account.events";
        private const string AuditQueueName = "account.audit";
        private const string AntifraudQueueName = "account.antifraud";
        private async Task Init()
        {
            factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Hostname"]!, 
                UserName = configuration["RabbitMQ:Username"]!, 
                Password = configuration["RabbitMQ:Password"]!,
            };
            connection = await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();
            
            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );
            
            await channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic);
            await SetupQueues();
            
            AsyncEventingBasicConsumer auditConsumer = new(channel);
            auditConsumer.ReceivedAsync += ProcessAuditMessage;
            
            AsyncEventingBasicConsumer antifraudConsumer = new(channel);
            antifraudConsumer.ReceivedAsync += ProcessAntifraudMessage;

            await channel.BasicConsumeAsync(AntifraudQueueName, autoAck: false, consumer: antifraudConsumer);
            await channel.BasicConsumeAsync(AuditQueueName, autoAck: false, consumer: auditConsumer);
        }

        private async Task ProcessAntifraudMessage(object model, BasicDeliverEventArgs ea)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IBankAccountsDbContext>();
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);  
            logger.LogInformation($"Received {message}");
        }

        private async Task DeadLetter(IBankAccountsDbContext dbContext, JsonDocument message, EventType? eventType, BasicDeliverEventArgs ea, string reason)
        {
            
            LogDeadLetter(message, eventType, reason);
            await AddToDeadLetters(dbContext, message.RootElement.GetRawText(), eventType, DateTime.UtcNow, "Receiver",
                reason);
            await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        }

        private async Task ProcessAuditMessage(object model, BasicDeliverEventArgs ea)
        {
            using var scope = scopeFactory.CreateScope();
            IBankAccountsDbContext dbContext = scope.ServiceProvider.GetRequiredService<IBankAccountsDbContext>();
            byte[] body = ea.Body.ToArray();
            string message = BytesToString(body);
            JsonDocument document = JsonDocument.Parse(message);
            
            bool hasHeaders = false;
            
            EventType? nullableEventType = null;
            
            string headerCausationId = null!;
            string headerCorrelationId = null!;

            string version = null!;
            string bodyCausationId = null!;
            string bodyCorrelationId = null!;

            string reason = null!;
            
            try
            {
                IDictionary<string, object?> headers = ea.BasicProperties.Headers!;
                hasHeaders = headers.Count != 0;
                
                EventType eventType = Enum.Parse<EventType>(BytesToString(headers["type"]!));
                nullableEventType = eventType;
                
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
                    throw new Exception();
                }

                if (headerCorrelationId != bodyCorrelationId)
                {
                    reason = "Correlation id mismatch";
                    throw new Exception();
                }

                if (int.Parse(version.Substring(1, 1)) < 1) // проверка на соответствие версии
                {
                    reason = "Message version not supported";
                    throw new Exception();
                }
                
                InboxConsumedEntity? result = await AddToInbox(dbContext, eventType, DateTime.UtcNow, "Receiver");
                    
                if (result != null)
                {
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    var propTimestamp = ea.BasicProperties.Timestamp.UnixTime;
                    var timestamp = propTimestamp == 0 ? (DateTime?) null : DateTime.FromBinary(propTimestamp);
                    LogSuccess(document, eventType, timestamp);
                }
                else
                {
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            
            }
            catch (Exception)
            {
                if (reason is null)
                {
                    if (!hasHeaders)
                        reason = "No headers";
                    else if (headerCausationId is null)
                        reason = "No header causation";
                    else if (headerCorrelationId is null)
                        reason = "No header correlation";
                    else if (version is null)
                        reason = "No version";
                    else if (bodyCausationId is null)
                        reason = "No body causation";
                    else if (bodyCorrelationId is null)
                        reason = "No body correlation";
                    else 
                        reason = "Unknown reason";
                }
                
                await DeadLetter(dbContext, document, nullableEventType, ea, reason);
            }
        }

        private string BytesToString(object bytes)
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
            catch (Exception) {/* ignored */ }
            
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

        private async Task<InboxConsumedEntity?> AddToInbox(IBankAccountsDbContext dbContext, EventType eventType, DateTime processedAt, string handler)
        {
            InboxConsumedEntity? entity = null;
            try
            {
                InboxConsumedEntity inboxConsumed = new()
                {
                    EventType = eventType, 
                    ProcessedAt = processedAt, 
                    Handler = handler
                };
            
                entity = (await dbContext.InboxConsumed.AddAsync(inboxConsumed)).Entity;
                await dbContext.SaveChangesAsync(CancellationToken.None);

            } catch (Exception e) { logger.LogError("{Message}",
                "Failed to add message to inbox.\n" + e.Message + "\n" + e.StackTrace); }
            
            return entity;
        }
        
        private async Task<DeadLetterEntity?> AddToDeadLetters(IBankAccountsDbContext dbContext, string message, EventType? eventType, 
            DateTime recievedAt, string handler, string error)
        {
            DeadLetterEntity? entity = null;
            
            try {
                DeadLetterEntity deadLetter = new()
                {
                    ReceivedAt = recievedAt,
                    Handler = handler,
                    Payload = message,
                    EventType = eventType,
                    Error = error,
                };
                
                entity = (await dbContext.DeadLetters.AddAsync(deadLetter)).Entity;
                await dbContext.SaveChangesAsync(CancellationToken.None);
                
            } catch (Exception e) { logger.LogError("{Message}",
                "Failed to add message to dead letters.\n" + e.Message + "\n" + e.StackTrace); }
            
            return entity;
        }
        
        /// <summary>
        /// Создаст очереди принимающие события по типам для наглядного представления отправленных сообщений
        /// </summary>
        private async Task SetupQueues()
        {
            await channel.QueueDeclareAsync(
                queue: AntifraudQueueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false,
                arguments: null);
            
            await channel.QueueBindAsync(AntifraudQueueName, ExchangeName, "client.#");

            await channel.QueueDeclareAsync(
                queue: AuditQueueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false,
                arguments: null
            );
            
            await channel.QueueBindAsync(AuditQueueName, ExchangeName, "#");
            
            foreach (EventType type in Enum.GetValues<EventType>())
            {
                if (type is EventType.ClientBlocked or EventType.ClientUnblocked)
                    continue;
                
                await channel!.QueueDeclareAsync(queue: $"test_{type.ToString()}", durable: true, false, false);
                await channel!.QueueBindAsync($"test_{type.ToString()}", ExchangeName, Event.GetRoute(type));
            }
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Init();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        { 
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Null в исключительных случаях
            if (channel != null)
                await channel.DisposeAsync();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Null в исключительных случаях
            if (connection != null)
                await connection.DisposeAsync();
        }
    }
}