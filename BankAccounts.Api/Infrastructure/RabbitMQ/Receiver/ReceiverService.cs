using BankAccounts.Api.Infrastructure.RabbitMQ.Receiver.Handlers.Antifraud;
using BankAccounts.Api.Infrastructure.RabbitMQ.Receiver.Handlers.Audit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Receiver
{
    /// <summary>
    /// Обработчик входящих сообщений RabbitMQ
    /// </summary>
    /// <param name="scopeFactory"></param>
    /// <param name="configuration"></param>
    public class ReceiverService(IServiceScopeFactory scopeFactory, IConfiguration configuration) 
        : IHostedService
    {
        private ConnectionFactory _factory = null!;
        private IConnection _connection = null!;
        private IChannel _channel = null!;
        private readonly string _exchangeName = configuration["RabbitMQ:ExchangeName"]!;
        private const string AuditQueueName = "account.audit";
        private const string AntifraudQueueName = "account.antifraud";
        private const string AccountCrmQueueName = "account.crm";
        private const string AccountNotificationsQueueName = "account.notifications";


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
        private async Task ProcessAuditMessage(object model, BasicDeliverEventArgs ea)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IAuditMessageHandler auditHandler = scope.ServiceProvider.GetRequiredService<IAuditMessageHandler>();
            await auditHandler.ProcessAuditMessage(_channel, ea);
        }

        /// <summary>
        /// Обработка сообщений о блокировке/разблокировке
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ea"></param>
        private async Task ProcessAntifraudMessage(object model, BasicDeliverEventArgs ea)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IAntifraudMessageHandler auditHandler = scope.ServiceProvider.GetRequiredService<IAntifraudMessageHandler>();
            await auditHandler.ProcessAntifraudMessage(_channel, ea);
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

            await _channel.QueueDeclareAsync(
                queue: AccountCrmQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            await _channel.QueueBindAsync(AccountCrmQueueName, _exchangeName, "account.*");

            await _channel.QueueDeclareAsync(
                queue: AccountNotificationsQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            await _channel.QueueBindAsync(AccountNotificationsQueueName, _exchangeName, "money.*");
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