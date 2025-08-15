using BankAccounts.Api.Common;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace BankAccounts.Api.Infrastructure.Hangfire.Jobs
{
    // Рекомендуется использовать Retry с экспоненциальной паузой и джиттером; при достижении MAX_RETRIES — статус dead‑letter, алерт в логах.
    public class Sender(ILogger<Sender> logger, IBankAccountsDbContext dbContext)
    {
        const ushort MAX_OUTSTANDING_CONFIRMS = 256;
        int MESSAGE_COUNT = 1000;
        ConnectionFactory factory = new() { HostName = "rabbitmq", UserName = "admin", Password = "admin"};
        private IConnection? connection;
        private IChannel? channel;
        private const string ExchangeName = "account.events";
        
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
            await channel.QueueDeclareAsync(queue: "test", false, false);
            
            await channel.QueueBindAsync("test", ExchangeName, "account.*");
        }

        public async Task Job()
        {
            if (connection is null || connection.IsOpen == false || channel is null || channel.IsClosed == false)
               await Init();
            
            Stopwatch sw = Stopwatch.StartNew();

            List<ValueTask> publishTasks = new List<ValueTask>();
            //var successfulGuids = new List<Guid>();

            List<OutboxPublishedEntity> entities = await dbContext.OutboxPublished.ToListAsync();
            int entitiesCount = entities.Count;
            
            int publishedCount = await Send(publishTasks, entities);

            // if (publishTasks.Count != 0 
            //     && await RetryWithJitterAsync<Task>(() => 
            //             Send(publishTasks, successfulGuids), 
            //             () => publishTasks.Count == 0))
            // {
                 sw.Stop();
                 logger.LogInformation($"{DateTime.Now} [INFO] published {publishedCount} messages (should {entitiesCount}) in batch in {sw.ElapsedMilliseconds:N0} ms");
            // }
            // else if (publishTasks.Count != 0)
            // {
            //     sw.Stop();
            //     logger.LogError($"{DateTime.Now} Не удалось опубликовать {publishTasks.Count} задач. Попытки прекращаются.");
            // }
        }

        private async Task<int> Send(List<ValueTask> publishTasks, List<OutboxPublishedEntity> entities)
        {
            var props = new BasicProperties
            {
                Persistent = true,
                Type = "AccountOpened"
            };
                
            // AccountOpened accountOpened = new(Guid.NewGuid(), DateTime.UtcNow, new Metadata(), 1, 
            //     Guid.NewGuid(), Currencies.Rub, AccountType.Checking);
            
            
            
            int batchSize = Math.Max(1, MAX_OUTSTANDING_CONFIRMS / 2);
            int succededPublishes = 0;
            
            foreach (OutboxPublishedEntity entity in entities)
            {
                object serviceEvent = GetEventFromJson(entity);
                ((Event)serviceEvent).EventId = entity.Id;
                string message = JsonObjectSerializer.ToJson(serviceEvent);
                var body = Encoding.UTF8.GetBytes(message);
                
                ValueTask publishTask = channel!.BasicPublishAsync(exchange: ExchangeName, routingKey: Event.GetRoute(EventType.AccountOpened), 
                    body: body, basicProperties: props, mandatory: false);
                publishTasks.Add(publishTask);
                
                succededPublishes += await MaybeAwaitPublishes(publishTasks, batchSize, logger);
            }

            // Await any remaining tasks in case message count was not
            // evenly divisible by batch size.
            succededPublishes += await MaybeAwaitPublishes(publishTasks, 0, logger);
            
            return succededPublishes;
        }

        private Event GetEventFromJson(OutboxPublishedEntity entity)
        {
            EventType type = entity.EventType;
            string json = entity.Message;
            
            switch (type)
            {
                case EventType.AccountOpened:
                return JsonObjectSerializer.FromJson<AccountOpened>(json)!;
            }

            logger.LogError("Wrong event type");
            throw new InvalidOperationException("Wrong entity type");
        }
        
        static async Task<int> MaybeAwaitPublishes(List<ValueTask> publishTasks, int batchSize, ILogger logger)
        {
            int succedeedTasks = 0;
            if (publishTasks.Count >= batchSize)
            {
                foreach (ValueTask pt in publishTasks)
                {
                    try
                    {
                        await pt;
                        succedeedTasks++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{DateTime.Now} [ERROR] saw nack or return, ex: '{ex}'");
                    }
                }
                publishTasks.Clear();
            }
            return succedeedTasks;
        }
        
        // private async Task<bool> RetryWithJitterAsync<T>(
        //     Func<Task> operation,
        //     Func<bool> condition,
        //     int maxRetries = 10,
        //     int baseDelayMs = 1000)
        // {
        //     var random = new Random();
        //
        //     for (int attempt = 0; attempt <= maxRetries; attempt++)
        //     {
        //         await operation();
        //         
        //         if (condition())
        //             return true;
        //     
        //         // Экспоненциальная задержка с джиттером
        //         var delay = baseDelayMs * Math.Pow(2, attempt);
        //         var jitter = random.Next((int)(delay * 0.5), (int)(delay * 1.5));
        //
        //         Console.WriteLine($"Попытка {attempt + 1} не удалась. Пауза: {jitter} мс");
        //         await Task.Delay(jitter);
        //     }
        //
        //     return false;
        // }
        
        // private TaskCompletionSource<bool> allMessagesConfirmedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        // private LinkedList<ulong> outstandingConfirms = new LinkedList<ulong>();
        // private SemaphoreSlim semaphore = new(1, 1);
        // int confirmedCount = 0;
        // bool debug = true;
        // async Task HandlePublishConfirmsAsynchronously()
        // {
        //     Console.WriteLine($"{DateTime.Now} [INFO] publishing {MESSAGE_COUNT:N0} messages and handling confirms asynchronously");
        //     
        //     // declare a server-named queue
        //     QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
        //     string queueName = queueDeclareResult.QueueName;
        //
        //     channel.BasicReturnAsync += (sender, ea) =>
        //     {
        //         ulong sequenceNumber = 0;
        //
        //         IReadOnlyBasicProperties props = ea.BasicProperties;
        //         if (props.Headers is not null)
        //         {
        //             object? maybeSeqNum = props.Headers[Constants.PublishSequenceNumberHeader];
        //             if (maybeSeqNum is not null)
        //             {
        //                 sequenceNumber = BinaryPrimitives.ReadUInt64BigEndian((byte[])maybeSeqNum);
        //             }
        //         }
        //
        //         Console.WriteLine($"{DateTime.Now} [WARNING] message sequence number {sequenceNumber} has been basic.return-ed");
        //         return CleanOutstandingConfirms(sequenceNumber, false);
        //     };
        //     channel.BasicAcksAsync += (sender, ea) => CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
        //     channel.BasicNacksAsync += (sender, ea) =>
        //     {
        //         Console.WriteLine($"{DateTime.Now} [WARNING] message sequence number: {ea.DeliveryTag} has been nacked (multiple: {ea.Multiple})");
        //         return CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
        //     };
        //
        //     var sw = new Stopwatch();
        //     sw.Start();
        //
        //     var publishTasks = new List<ValueTuple<ulong, ValueTask>>();
        //     for (int i = 0; i < MESSAGE_COUNT; i++)
        //     {
        //         string msg = i.ToString();
        //         byte[] body = Encoding.UTF8.GetBytes(msg);
        //         ulong nextPublishSeqNo = await channel.GetNextPublishSequenceNumberAsync();
        //         if ((ulong)(i + 1) != nextPublishSeqNo)
        //         {
        //             Console.WriteLine($"{DateTime.Now} [WARNING] i {i + 1} does not equal next sequence number: {nextPublishSeqNo}");
        //         }
        //         await semaphore.WaitAsync();
        //         try
        //         {
        //             outstandingConfirms.AddLast(nextPublishSeqNo);
        //         }
        //         finally
        //         {
        //             semaphore.Release();
        //         }
        //         
        //         (ulong, ValueTask) data =
        //             (nextPublishSeqNo, channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body, mandatory: true, basicProperties: props));
        //         publishTasks.Add(data);
        //     }
        //
        //     using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        //     // await Task.WhenAll(publishTasks).WaitAsync(cts.Token);
        //     foreach ((ulong SeqNo, ValueTask PublishTask) datum in publishTasks)
        //     {
        //         try
        //         {
        //             await datum.PublishTask;
        //         }
        //         catch (Exception ex)
        //         {
        //             Console.Error.WriteLine($"{DateTime.Now} [ERROR] saw nack, seqNo: '{datum.SeqNo}', ex: '{ex}'");
        //         }
        //     }
        //
        //     try
        //     {
        //         await allMessagesConfirmedTcs.Task.WaitAsync(cts.Token);
        //     }
        //     catch (OperationCanceledException)
        //     {
        //         Console.Error.WriteLine("{0} [ERROR] all messages could not be published and confirmed within 10 seconds", DateTime.Now);
        //     }
        //     catch (TimeoutException)
        //     {
        //         Console.Error.WriteLine("{0} [ERROR] all messages could not be published and confirmed within 10 seconds", DateTime.Now);
        //     }
        //
        //     sw.Stop();
        //     Console.WriteLine($"{DateTime.Now} [INFO] published {MESSAGE_COUNT:N0} messages and handled confirm asynchronously {sw.ElapsedMilliseconds:N0} ms");
        // }
        //
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // /// <param name="deliveryTag"></param>
        // /// <param name="multiple"></param>
        // async Task CleanOutstandingConfirms(ulong deliveryTag, bool multiple)
        // {
        //     if (debug)
        //     {
        //         Console.WriteLine("{0} [DEBUG] confirming message: {1} (multiple: {2})",
        //             DateTime.Now, deliveryTag, multiple);
        //     }
        //
        //     await semaphore.WaitAsync();
        //     try
        //     {
        //         if (multiple)
        //         {
        //             do
        //             {
        //                 LinkedListNode<ulong>? node = outstandingConfirms.First;
        //                 if (node is null)
        //                 {
        //                     break;
        //                 }
        //                 if (node.Value <= deliveryTag)
        //                 {
        //                     outstandingConfirms.RemoveFirst();
        //                 }
        //                 else
        //                 {
        //                     break;
        //                 }
        //
        //                 confirmedCount++;
        //             } while (true);
        //         }
        //         else
        //         {
        //             confirmedCount++;
        //             outstandingConfirms.Remove(deliveryTag);
        //         }
        //     }
        //     finally
        //     {
        //         semaphore.Release();
        //     }
        //
        //     if (outstandingConfirms.Count == 0 || confirmedCount == MESSAGE_COUNT)
        //     {
        //         allMessagesConfirmedTcs.SetResult(true);
        //     }
        // }
    }
}