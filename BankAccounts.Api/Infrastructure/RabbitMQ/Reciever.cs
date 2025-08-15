using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace BankAccounts.Api.Infrastructure.RabbitMQ
{
    public class Reciever(ILogger<Reciever> logger) : IHostedService
    {

        private async Task Init()
        {
            ConnectionFactory factory = new() {  HostName = "rabbitmq", UserName = "admin", Password = "admin" };
            using IConnection connection = await factory.CreateConnectionAsync();
            using IChannel channel = await connection.CreateChannelAsync();
            
            
            logger.LogInformation(new ConfigureSwaggerOptions().GetType().ToString());

            await channel.QueueDeclareAsync(queue: "queue", 
                durable: true, 
                exclusive: false, 
                 autoDelete: false,
                arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            AsyncEventingBasicConsumer consumer = new(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                logger.LogInformation($" [x] Received {message}");
                // ReSharper disable once AccessToDisposedClosure
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);  
            };

            await channel.BasicConsumeAsync("queue", autoAck: false, consumer: consumer);
            
          
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Init();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}