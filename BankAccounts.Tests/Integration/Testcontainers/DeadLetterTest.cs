using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Tests.Integration.Testcontainers.Utility;
using BankAccounts.Tests.Integration.Testcontainers.Utility.Factories;
using BankAccounts.Tests.Integration.Testcontainers.Utility.Json;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using RabbitMQ.Client;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace BankAccounts.Tests.Integration.Testcontainers
{
    public partial class DeadLetterTest : IAsyncLifetime
    {
        private INetwork _network = null!;
        private PostgreSqlContainer _bankAccountsDbContainer = null!;
        private IContainer _bankAccountsApiContainer = null!; 
        private RabbitMqContainer _rabbitMqContainer = null!;
        
        private MessageHelper _messageHelper = null!;
        
        private HttpClient _apiHttpClient = null!;
        
        public async Task InitializeAsync()
        {
            ContainerFactory factory = await new ContainerFactory().InitializeAndAndBuildAsync();
            
            _network = factory.Network;
            _bankAccountsDbContainer = await factory.BuildPostgresContainer();
            _rabbitMqContainer = await factory.BuildRabbitMqContainer();
            _bankAccountsApiContainer = await factory.BuildApiContainer(_bankAccountsDbContainer, _rabbitMqContainer);
            
            _apiHttpClient = ClientBuilder.GetTestApiClient(_bankAccountsApiContainer);
            
            _messageHelper = await new MessageHelper().Init(_rabbitMqContainer);
        }

        [Fact]
        public async Task ConsumeDeadLetter_ReceiverConsumesAndLogs()
        {
            // Arrange
            AccountOpened serviceEvent = new AccountOpened()
            {
                Meta = new Metadata
                {
                    CausationId = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                    Version = "v1",
                    Source = "accounts-service"
                },
                OwnerId = Guid.Parse("0452b2ec-5e4b-f650-b9ee-78ec1a129047")
            };
            
            BasicProperties propsNoMessageId = _messageHelper.CreateBasicProperties(serviceEvent);
            propsNoMessageId.MessageId = null;
            
            BasicProperties propsNoCorrelationId = _messageHelper.CreateBasicProperties(serviceEvent);
            propsNoCorrelationId.Headers!["x-correlation-id"] = null;
            
            // Act
            await _messageHelper.SendMessage(serviceEvent, propsNoMessageId);
            await _messageHelper.SendMessage(serviceEvent, propsNoCorrelationId);
            await _messageHelper.SendMessage(serviceEvent, propsNoMessageId);
            await _messageHelper.SendMessage(serviceEvent, propsNoCorrelationId);
            await _messageHelper.SendMessage(serviceEvent, propsNoMessageId);
            await _messageHelper.SendMessage(serviceEvent, propsNoCorrelationId);
            
            // Assert
            
        }
        
        [GeneratedRegex("No messageId")]
        private static partial Regex ClientBlockedReg();
        [GeneratedRegex("No correlationId")]
        private static partial Regex ClientUnblockedReg();
        
        private async Task WaitMessageConsumed(Regex regex)
        {
            bool matchNotConsumed = true;

            while (matchNotConsumed)
            {
                await Task.Delay(100);
                (string, string) logs = await _bankAccountsApiContainer.GetLogsAsync();
                string stdout = logs.Item1;
                matchNotConsumed = !regex.IsMatch(stdout);
            }
        }

        public async Task DisposeAsync()
        {
            await _bankAccountsDbContainer.DisposeAsync();
            await _rabbitMqContainer.DisposeAsync();
            await _bankAccountsDbContainer.DisposeAsync();
            await _network.DisposeAsync();
            _apiHttpClient.Dispose();
        }
    }
}
