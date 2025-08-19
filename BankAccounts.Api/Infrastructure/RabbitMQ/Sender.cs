using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace BankAccounts.Api.Infrastructure.RabbitMQ
{
    
    /// <summary>
    /// Отправщик событий
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbContext"></param>
    /// <param name="configuration"></param>
    public class Sender(ILogger<Sender> logger, IBankAccountsDbContext dbContext, IConfiguration configuration)
    {
        private const ushort MaxOutstandingConfirms = 256;

        private readonly ConnectionFactory _factory = new()
        {
            HostName = configuration["RabbitMQ:Hostname"]!, 
            UserName = configuration["RabbitMQ:Username"]!, 
            Password = configuration["RabbitMQ:Password"]!
        };
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _exchangeName = configuration["RabbitMQ:ExchangeName"]!;
        private readonly BasicProperties _props = new()
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>()
        };
        
        /// <summary>
        /// Инициализация подключения и канала
        /// </summary>
        private async Task Init()
        {
            _connection = await _factory.CreateConnectionAsync();
            
            CreateChannelOptions channelOptions = new (
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true,
                outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(MaxOutstandingConfirms)
            );
            
            _channel = await _connection.CreateChannelAsync(channelOptions);
            
            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );
        }

        /// <summary>
        /// Выполнение отправки сообщений из outbox
        /// </summary>
        [AutomaticRetry(Attempts = 0)]
        public async Task Job()
        {
            if (_connection is null || _connection.IsOpen == false || _channel is null || _channel.IsClosed == false)
               await Init();
            
            Stopwatch sw = Stopwatch.StartNew();

            List<(OutboxPublishedEntity, ValueTask)> publishTasks = [];

            List<OutboxPublishedEntity> entities = await dbContext.OutboxPublished.ToListAsync();
            
            int publishedCount = await Send(publishTasks, entities);
            
             sw.Stop();
             int count = await dbContext.OutboxPublished.CountAsync();
             if (count > 0) // выводим сообщение в случае, если не все сообщения были отправлены
                logger.LogInformation("Published {PublishedCount} messages (failed and queued for retry: {Count}) " +
                        "in batch in {SwElapsedMilliseconds:N0} ms", publishedCount, count, sw.ElapsedMilliseconds);
             await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        private async Task<int> Send(List<(OutboxPublishedEntity, ValueTask)> publishTasks, List<OutboxPublishedEntity> entities)
        {
            int batchSize = Math.Max(1, MaxOutstandingConfirms / 2);
            int succeededPublishes = 0;
            
            foreach (OutboxPublishedEntity entity in entities)
            {
                string message = entity.Message;
                byte[] body = Encoding.UTF8.GetBytes(message);
                
                _props.Headers!["type"] = entity.EventType.ToString();
                _props.Headers["x-correlation-id"] = entity.CorrelationId.ToString();
                _props.Headers["x-causation-id"] = entity.CausationId.ToString();
                _props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToBinary());
                _props.MessageId = entity.Id.ToString();
                
                ValueTask publishTask = _channel!.BasicPublishAsync(exchange: _exchangeName, routingKey: Event.GetRoute(entity.EventType), 
                    body: body, basicProperties: _props, mandatory: false);
                publishTasks.Add((entity, publishTask));
                
                succeededPublishes += await MaybeAwaitPublishes(publishTasks, batchSize);
            }

            // Await any remaining tasks in case message count was not
            // evenly divisible by batch size.
            succeededPublishes += await MaybeAwaitPublishes(publishTasks, 0);
            
            return succeededPublishes;
        }

        private async Task<int> MaybeAwaitPublishes(List<(OutboxPublishedEntity, ValueTask)> publishTasks, int batchSize)
        {
            int succeededTasks = 0;
            
            if (publishTasks.Count < batchSize)
            {
                return succeededTasks;
            }

            foreach ((OutboxPublishedEntity entity, ValueTask pt)  in publishTasks)
            {
                try
                {
                    await pt;
                    dbContext.OutboxPublished.Remove(entity);
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                    succeededTasks++;
                    LogSuccess(entity);
                }
                catch (Exception)
                {
                    //logger.LogError("Error during event publishing ({Tries}): '{Exception}'", entity.TryCount, ex);
                    entity.TryCount += 1;
                    dbContext.OutboxPublished.Update(entity);
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                }
            }
            publishTasks.Clear();
            return succeededTasks;
        }

        private void LogSuccess(OutboxPublishedEntity entity)
        {
            logger.LogInformation("[MESSAGE_PUBLISHED] Successfully published event: id = {id}, type = {type}, " +
                                  "correlationId = {correlationId}, retry = {retry}, latency = {latency}", entity.EventId, 
                entity.EventType.ToString(), entity.CorrelationId, entity.TryCount, DateTime.UtcNow - entity.Created);
        }
    }
}