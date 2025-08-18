using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Dtos;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace BankAccounts.Tests.Integration.Testcontainers
{
    public partial class CrossServiceIntegrationTests
    {
        [Fact]
        public async Task SenderTest_OutboxPublishesAfterFailure()
        {
            // Arrange
            using ApiClientHelper clientHelper = new(_identityHttpClient, _apiHttpClient);
            using HttpClient apiClientUser1 = await clientHelper.AuthorizeApiHttpClient("user1", "password");
            const decimal initialBalanceUser1 = 10000m; 

            await _rabbitMqContainer.PauseAsync(); // имитируем недоступность RabbitMQ
            
            //Act
            int account1Id = await ApiClientHelper.CreateAccount(apiClientUser1);
            
            // Производим транзакцию
            PerformTransactionDto depositDto1 = new(TransactionType.Debit, initialBalanceUser1, "Начальное пополнение");
            await apiClientUser1.PostAsJsonAsync($"/api/accounts/{account1Id}/transactions", depositDto1);

            // Ждем 10 секунд, потому что сообщения отправляются каждые 10 секунд, таким образом гарантируем,
            // что сообщения не смогут быть отправлены в следующую отправку 
            await Task.Delay(10000); 

            // Восстанавливаем работоспособность сервиса RabbitMQ
            await _rabbitMqContainer.UnpauseAsync();
            
            // Ждем, чтобы сообщения отправились
            await Task.Delay(10000);
            
            // Assert
            (string, string) logs = await _bankAccountsApiContainer.GetLogsAsync();
            string stdout = logs.Item1;
            // Проверяем, что в логах API есть сообщения об успешной обработке сообщений
            bool matchPublished = MessagePublished().IsMatch(stdout);
            bool matchConsumed = MessageConsumed().IsMatch(stdout);
            
            Assert.True(matchPublished);
            Assert.True(matchConsumed);
        }

        [GeneratedRegex("Successfully consumed event.+AccountOpened")]
        private static partial Regex MessageConsumed();
        [GeneratedRegex("Successfully published event.+AccountOpened")]
        private static partial Regex MessagePublished();
    }
}
