using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Tests.Integration.Testcontainers.Utility.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using Testcontainers.RabbitMq;

namespace BankAccounts.Tests.Integration.Testcontainers.Utility.Factories
{
    public class MessageHelper : IAsyncDisposable
    {
        private IConnection _connection = null!;
        private IChannel _channel = null!;

        private RabbitMqContainer _rabbitMqContainer = null!;
        //private IConfiguration _configuration = null!;
        
        private JsonHelper  _jsonHelper = null!;

        public async Task<MessageHelper> Init(RabbitMqContainer mqCont)
        {
            _rabbitMqContainer = mqCont;
            _jsonHelper = JsonHelperBuilder.BuildHelper();
            
            ConnectionFactory factory = new()
            {
                HostName = "localhost",
                Port = mqCont.GetMappedPublicPort(5672),
                UserName = "admin",
                Password = "admin"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            
            return this;
        }
        
        // private IConfiguration BuildConfiguration(string name)
        // {
        //     IConfigurationBuilder builder = new ConfigurationBuilder()
        //         .SetBasePath(Directory.GetCurrentDirectory())
        //         .AddJsonFile(name, optional: false, reloadOnChange: true)
        //         .AddEnvironmentVariables(); 
        //     
        //     return builder.Build();
        // }
        
        public async Task SendMessage(Event serviceEvent)
        {
            string message = _jsonHelper.ToJson(serviceEvent);
            byte[] body = Encoding.UTF8.GetBytes(message);
            EventType eventType = Event.GetEventType(serviceEvent);
                
            BasicProperties props = new() { Headers = new Dictionary<string, object?>() };
            props.Headers!["type"] = eventType;
            props.Headers["x-correlation-id"] = serviceEvent.Meta.CorrelationId;
            props.Headers["x-causation-id"] = serviceEvent.Meta.CausationId;
            props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToBinary());
            props.MessageId = GetNewGuidString();
            
            await _channel.BasicPublishAsync(exchange: "account.events", routingKey: Event.GetRoute(eventType), 
                body: body, basicProperties: props, mandatory: false);
        }

        public BasicProperties CreateBasicProperties(Event serviceEvent)
        {
            EventType eventType  = Event.GetEventType(serviceEvent);
            
            BasicProperties props = new() { Headers = new Dictionary<string, object?>() };
            props.Headers!["type"] = eventType;
            props.Headers["x-correlation-id"] = serviceEvent.Meta.CorrelationId;
            props.Headers["x-causation-id"] = serviceEvent.Meta.CausationId;
            props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToBinary());
            props.MessageId = GetNewGuidString();
            
            return props;
        }

        public async Task SendMessage<T>(T serviceEvent, BasicProperties props) where T : Event
        {
            string message = _jsonHelper.ToJson(serviceEvent);
            byte[] body = Encoding.UTF8.GetBytes(message);
            EventType eventType = Event.GetEventType(serviceEvent); 
            
            await _channel.BasicPublishAsync(exchange: "account.events", routingKey: Event.GetRoute(eventType), 
                body: body, basicProperties: props, mandatory: false);
        }

        public string GetNewGuidString()
        {
           return Guid.NewGuid().ToString();
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
            await _channel.DisposeAsync();
            await _rabbitMqContainer.DisposeAsync();
        }
    }
}
