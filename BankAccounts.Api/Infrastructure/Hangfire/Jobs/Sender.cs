using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;

namespace BankAccounts.Api.Infrastructure.Hangfire.Jobs
{
    // Рекомендуется использовать Retry с экспоненциальной паузой и джиттером; при достижении MAX_RETRIES — статус dead‑letter, алерт в логах.
    public class Sender(ILogger<Sender> logger, IBankAccountsDbContext dbContext, IConfiguration configuration)
    {
        const ushort MAX_OUTSTANDING_CONFIRMS = 256;
        int MESSAGE_COUNT = 1000;
        ConnectionFactory factory = new() { HostName = "rabbitmq", UserName = "admin", Password = "admin"};
        private IConnection? connection;
        private IChannel? channel;
        private const string ExchangeName = Receiver.ExchangeName;
        BasicProperties props = new()
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>()
        };
        
        public async Task Init()
        {
            connection = await factory.CreateConnectionAsync();
            
            CreateChannelOptions channelOptions = new (
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true,
                outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(MAX_OUTSTANDING_CONFIRMS)
            );
            
            channel = await connection.CreateChannelAsync(channelOptions);
            
            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );
            
            await channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic);
            await SetupQueues();
        }

        /// <summary>
        /// Создаст очереди принимающие события по типам для наглядного представления отправленных сообщений
        /// </summary>
        private async Task SetupQueues()
        {
            foreach (EventType type in Enum.GetValues<EventType>())
            {
                if (type is EventType.ClientBlocked or EventType.ClientUnblocked)
                    continue;
                
                await channel!.QueueDeclareAsync(queue: $"test_{type.ToString()}", false, false);
                await channel!.QueueBindAsync($"test_{type.ToString()}", ExchangeName, Event.GetRoute(type));
            }
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Job()
        {
            if (connection is null || connection.IsOpen == false || channel is null || channel.IsClosed == false)
               await Init();
            
            Stopwatch sw = Stopwatch.StartNew();

            List<(OutboxPublishedEntity, ValueTask)> publishTasks = new();
            //var successfulGuids = new List<Guid>();

            List<OutboxPublishedEntity> entities = await dbContext.OutboxPublished.ToListAsync();
            
            int publishedCount = await Send(publishTasks, entities);

            // if (publishTasks.Count != 0 
            //     && await RetryWithJitterAsync<Task>(() => 
            //             Send(publishTasks, successfulGuids), 
            //             () => publishTasks.Count == 0))
            // {
                 sw.Stop();
                 var count = (await dbContext.OutboxPublished.CountAsync());
                 logger.LogInformation($"Published {publishedCount} messages (failed and queued for retry: {count}) in batch in {sw.ElapsedMilliseconds:N0} ms");
            // }
            // else if (publishTasks.Count != 0)
            // {
            //     sw.Stop();
            //     logger.LogError($"{DateTime.Now} Не удалось опубликовать {publishTasks.Count} задач. Попытки прекращаются.");
            // }
        }

        private async Task<int> Send(List<(OutboxPublishedEntity, ValueTask)> publishTasks, List<OutboxPublishedEntity> entities)
        {
            int batchSize = Math.Max(1, MAX_OUTSTANDING_CONFIRMS / 2);
            int succededPublishes = 0;
            
            foreach (OutboxPublishedEntity entity in entities)
            {
                string message = entity.Message;
                var body = Encoding.UTF8.GetBytes(message);
                props.Headers!["type"] = entity.EventType.ToString();
                props.Headers["x-correlation-id"] = entity.CorrelationId.ToString();
                props.Headers["x-causation-id"] = entity.CausationId.ToString();
                ValueTask publishTask = channel!.BasicPublishAsync(exchange: ExchangeName, routingKey: Event.GetRoute(entity.EventType), 
                    body: body, basicProperties: props, mandatory: false);
                publishTasks.Add((entity, publishTask));
                
                succededPublishes += await MaybeAwaitPublishes(publishTasks, batchSize, logger);
            }

            // Await any remaining tasks in case message count was not
            // evenly divisible by batch size.
            succededPublishes += await MaybeAwaitPublishes(publishTasks, 0, logger);
            
            return succededPublishes;
        }
        
        async Task<int> MaybeAwaitPublishes(List<(OutboxPublishedEntity, ValueTask)> publishTasks, int batchSize, ILogger logger)
        {
            int succedeedTasks = 0;
            if (publishTasks.Count >= batchSize)
            {
                foreach ((OutboxPublishedEntity entity, ValueTask pt)  in publishTasks)
                {
                    try
                    {
                        await pt;
                        dbContext.OutboxPublished.Remove(entity);
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                        succedeedTasks++;
                        LogSuccess(entity);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Error during event publishing: '{Exception}'", ex);
                        entity.TryCount += 1;
                        dbContext.OutboxPublished.Update(entity);
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                    }
                }
                publishTasks.Clear();
            }
            return succedeedTasks;
        }

        private void LogSuccess(OutboxPublishedEntity entity)
        {
            logger.LogInformation("Successfully published event: id = {id}, type = {type}, " +
                                  "correlationId = {correlationId}, retry = {retry}, latency = {latency}", entity.EventId, 
                entity.EventType.ToString(), entity.CorrelationId, entity.TryCount, DateTime.UtcNow - entity.Created); // eventId, type, correlationId, retry, latency
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deliveryTag"></param>
        /// <param name="multiple"></param>
        async Task CleanOutstandingConfirms(ulong deliveryTag, bool multiple)
        {
            if (debug)
            {
                Console.WriteLine("{0} [DEBUG] confirming message: {1} (multiple: {2})",
                    DateTime.Now, deliveryTag, multiple);
            }
        
            await semaphore.WaitAsync();
            try
            {
                if (multiple)
                {
                    do
                    {
                        LinkedListNode<ulong>? node = outstandingConfirms.First;
                        if (node is null)
                        {
                            break;
                        }
                        if (node.Value <= deliveryTag)
                        {
                            outstandingConfirms.RemoveFirst();
                        }
                        else
                        {
                            break;
                        }
        
                        confirmedCount++;
                    } while (true);
                }
                else
                {
                    confirmedCount++;
                    outstandingConfirms.Remove(deliveryTag);
                }
            }
            finally
            {
                semaphore.Release();
            }
        
            if (outstandingConfirms.Count == 0 || confirmedCount == MESSAGE_COUNT)
            {
                allMessagesConfirmedTcs.SetResult(true);
            }
        }
    }
}