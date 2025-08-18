using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using RabbitMQ.Client;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BankAccounts.Tests.Integration.Testcontainers
{
    public partial class CrossServiceIntegrationTests
    {
        private IConnection _connection = null!;
        private IChannel _channel = null!;

        public static readonly JsonSerializerOptions Options = new();
        
        [Fact]
        public async Task ClientBlockTest_ClientBlockedPreventsDebit()
        {
            // Arrange
            ConnectionFactory factory = new()
            {
                HostName = "localhost",
                Port = _rabbitMqContainer.GetMappedPublicPort(5672),
                UserName = "admin",
                Password = "admin"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            
            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );
            
            using ApiClientHelper clientHelper = new(_identityHttpClient, _apiHttpClient);
            using HttpClient apiClientUser1 = await clientHelper.AuthorizeApiHttpClient("test", "password");
            int accountId = await ApiClientHelper.CreateAccount(apiClientUser1);
            
            // Начальное пополнение
            await PerformTransaction(apiClientUser1, accountId, TransactionType.Debit, 300);
            
            //await WaitMessageConsumed(TransactionPerformedReg());

            // Act
            // Отправляем сообщение о блокировке
            await SendClientBlockedMessage();
            // Ждем обработки API
            await WaitMessageConsumed(ClientBlockedReg());
            
            // Пробуем снять средства
            HttpResponseMessage response1 = await PerformTransaction(apiClientUser1, accountId, TransactionType.Credit, 100);
            // Ждем обработки API
            //await WaitMessageConsumed(TransactionPerformedReg());

            // Отправляем сообщение о разблокировке
            await SendClientUnblockedMessage();
            // Ждем обработки API
            await WaitMessageConsumed(ClientUnblockedReg());
            
            // Пробуем снять средства после разблокировки пользователя
            HttpResponseMessage response2 = await PerformTransaction(apiClientUser1, accountId, TransactionType.Credit, 100);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        }
        
        [GeneratedRegex("CLIENT_BLOCK")]
        private static partial Regex ClientBlockedReg();
        [GeneratedRegex("CLIENT_UNBLOCK")]
        private static partial Regex ClientUnblockedReg();
        
        internal static async Task<HttpResponseMessage> PerformTransaction(HttpClient apiClientUser, int accountId, TransactionType transactionType, decimal amount)
        {
            PerformTransactionDto performTransactionDto = new(transactionType, amount, "");
            HttpResponseMessage performTransactionResponse = await apiClientUser.PostAsJsonAsync($"/api/accounts/{accountId}/transactions", performTransactionDto);
            return performTransactionResponse;
        }
        
        public static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }

        private async Task SendClientBlockedMessage()
        {
            Options.Converters.Add(new JsonStringEnumConverter());
            Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            Options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            string message = ToJson(new ClientBlocked
            {
                Meta = new Metadata
                {
                    CausationId = guid1,
                    CorrelationId = guid2,
                    Version = "v1",
                    Source = "accounts-service"
                },
                ClientId = Guid.Parse("e58f4aa9-b1cc-a65b-9c4c-0873d391e987")
            });
            byte[] body = Encoding.UTF8.GetBytes(message);
                
            BasicProperties props = new() { Headers = new Dictionary<string, object?>() };
            props.Headers!["type"] = "ClientBlocked";
            props.Headers["x-correlation-id"] = guid2.ToString();
            props.Headers["x-causation-id"] = guid1.ToString();
            props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToBinary());
            props.MessageId = Guid.NewGuid().ToString();
            
            await _channel.BasicPublishAsync(exchange: "account.events", routingKey: Event.GetRoute(EventType.ClientBlocked), 
                body: body, basicProperties: props, mandatory: false);
        }

        private async Task SendClientUnblockedMessage()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            string message = ToJson(new ClientUnblocked
            {
                Meta = new Metadata
                {
                    CausationId = guid1,
                    CorrelationId = guid2,
                    Version = "v1",
                    Source = "accounts-service"
                },
                ClientId = Guid.Parse("e58f4aa9-b1cc-a65b-9c4c-0873d391e987")
            });
            byte[] body = Encoding.UTF8.GetBytes(message);

            BasicProperties props = new() { Headers = new Dictionary<string, object?>() };
            props.Headers!["type"] = "ClientUnblocked";
            props.Headers["x-correlation-id"] = guid2.ToString();
            props.Headers["x-causation-id"] = guid1.ToString();
            props.Timestamp = new AmqpTimestamp(DateTime.UtcNow.ToBinary());
            props.MessageId = Guid.NewGuid().ToString();
            
            await _channel.BasicPublishAsync(exchange: "account.events", routingKey: Event.GetRoute(EventType.ClientUnblocked), 
                body: body, basicProperties: props, mandatory: false);
        }

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
            await Task.Delay(1000);
        }
        
    }
}
