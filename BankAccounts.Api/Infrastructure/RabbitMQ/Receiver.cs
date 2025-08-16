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
    public class Receiver(ILogger<Receiver> logger, IBankAccountsDbContext dbContext) : IHostedService
    {
        private ConnectionFactory factory;
        private IConnection connection;
        private IChannel channel;
        public const string ExchangeName = "account.events";
        private const string AuditQueueName = "account.audit";
        private const string AntifraudQueueName = "account.antifraud";
        private async Task Init()
        {
            factory = new ConnectionFactory {  HostName = "rabbitmq", UserName = "admin", Password = "admin" };
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
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);  
            logger.LogInformation($"Received {message}");
        }

        private async Task ProcessAuditMessage(object model, BasicDeliverEventArgs ea)
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            bool hasType = Enum.TryParse(ea.BasicProperties.Headers["type"].ToString(), out EventType eventType);
            if (ea.BasicProperties.Headers == null || ea.BasicProperties.Headers.Count == 0 || !hasType)
            {
                LogDeadLetter(message, hasType ? eventType : null);
                await AddToDeadLetters(message, hasType ? eventType : null, DateTime.UtcNow, "Receiver", "something went wrong");
                await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                return;
            }
            
            InboxConsumedEntity? result = await AddToInbox(eventType, DateTime.UtcNow, "Receiver");
            
            if (result != null)
            {
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                LogSuccess(message, eventType);
            }
            else
            {
                await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        }
        
        private void LogSuccess(string message, EventType type)
        {
            string? id = null;
            string? correlationId = null;
            DateTime createdAt = DateTime.MinValue;
            TimeSpan? latency = createdAt == DateTime.MinValue ? null : DateTime.UtcNow - createdAt;

            try
            {
                using JsonDocument document = JsonDocument.Parse(message);
                JsonElement root = document.RootElement;
                    
                id = root.GetProperty("eventid").GetString();
                correlationId = root.GetProperty("metadata").GetProperty("correlationid").GetString();
                DateTime.TryParse(root.GetProperty("createdAt").GetString(), out createdAt);
            }
            catch (Exception) {/* ignored */ }
                    
            logger.LogInformation("Successfully consumed event: id = {id}, type = {type}, " +
                                  "correlationId = {correlationId}, latency = {latency}", id, 
                type.ToString(), correlationId, latency); // eventId, type, correlationId, retry, latency
        }
        
        private void LogDeadLetter(string message, EventType? type)
        {
            string? id = null;
            string? correlationId = null;
            DateTime createdAt = DateTime.MinValue;
            TimeSpan? latency = createdAt == DateTime.MinValue ? null : DateTime.UtcNow - createdAt;

            try
            {
                using JsonDocument document = JsonDocument.Parse(message);
                JsonElement root = document.RootElement;
                    
                id = root.GetProperty("eventid").GetString();
                correlationId = root.GetProperty("metadata").GetProperty("correlationid").GetString();
                DateTime.TryParse(root.GetProperty("createdAt").GetString(), out createdAt);
            }
            catch (Exception) {/* ignored */ }
                    
            logger.LogInformation("Consumed \"dead letter\" event: id = {id}, type = {type}, " +
                                  "correlationId = {correlationId}, latency = {latency}", id, 
                type.ToString(), correlationId, latency); // eventId, type, correlationId, retry, latency
        }

        private async Task<InboxConsumedEntity?> AddToInbox(EventType eventType, DateTime processedAt, string handler)
        {
            InboxConsumedEntity? entity = null;
            try
            {
                InboxConsumedEntity inboxConsumed = new()
                {
                    EventType = eventType, ProcessedAt = processedAt, Handler = handler
                };
            
                entity = (await dbContext.InboxConsumed.AddAsync(inboxConsumed)).Entity;
                await dbContext.SaveChangesAsync(CancellationToken.None);

            } catch (Exception e) { logger.LogError("{Message}","Failed to add message to inbox.\n" + e.Message + "\n" + e.StackTrace); }
            
            return entity;
        }
        
        private async Task<DeadLetterEntity?> AddToDeadLetters(string message,EventType? eventType, DateTime recievedAt, string handler, string error)
        {
            DeadLetterEntity? entity = null;
            
            try {
                DeadLetterEntity deadLetter = new()
                {
                    RecievedAt = recievedAt,
                    Handler = handler,
                    Payload = message,
                    EventType = eventType,
                    Error = error,
                };
                
                entity = (await dbContext.DeadLetters.AddAsync(deadLetter)).Entity;
                await dbContext.SaveChangesAsync(CancellationToken.None);
                
            } catch (Exception e) { logger.LogError("{Message}","Failed to add message to dead letters.\n" + e.Message + "\n" + e.StackTrace); }
            
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
            
            //await channel.QueueBindAsync(AntifraudQueueName, ExchangeName, "client.#");

            await channel.QueueDeclareAsync(
                queue: AuditQueueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false,
                arguments: null
            );
            
            //await channel.QueueBindAsync(AuditQueueName, ExchangeName, "#");
            
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