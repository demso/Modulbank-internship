using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace BankAccounts.Tests.Integration.Testcontainers
{
    public partial class CrossServiceIntegrationTests
    {
        [Fact]
        public async Task ParallelTransferTests_TransferEmitsSingleEvent()
        {
            using ApiClientHelper clientHelper = new(_identityHttpClient, _apiHttpClient);

            // Регистрация пользователей
            // Пользователь 1
            using HttpClient apiClientUser1 = await clientHelper.AuthorizeApiHttpClient("test", "password");

            // Пользователь 2
            using HttpClient apiClientUser2 = await clientHelper.AuthorizeApiHttpClient("user2", "password");

            // Создание счетов
            int account1Id = await ApiClientHelper.CreateAccount(apiClientUser1);
            int account2Id = await ApiClientHelper.CreateAccount(apiClientUser2); 
            
            // Пополнение счетов начальными средствами
            const decimal initialBalanceUser1 = 10000m; 
            const decimal initialBalanceUser2 = 5000m; 
            const decimal totalInitialBalance = initialBalanceUser1 + initialBalanceUser2;

            // Пополнение счета 1
            PerformTransactionDto depositDto1 = new(TransactionType.Debit, initialBalanceUser1, "Начальное пополнение");
            await apiClientUser1.PostAsJsonAsync($"/api/accounts/{account1Id}/transactions", depositDto1);

            // Пополнение счета 2
            PerformTransactionDto depositDto2 = new(TransactionType.Debit, initialBalanceUser2, "Начальное пополнение");
            await apiClientUser2.PostAsJsonAsync($"/api/accounts/{account2Id}/transactions", depositDto2);

            // Проверка начальных балансов
            Assert.Equal(initialBalanceUser1, await ApiClientHelper.GetBalance(apiClientUser1, account1Id));
            Assert.Equal(initialBalanceUser2, await ApiClientHelper.GetBalance(apiClientUser2, account2Id));

            // Настройка параллельных переводов
            const decimal transferAmount = 10m; // Сумма одного перевода
            const int numberOfTransfers = 50; // Количество параллельных переводов

            PerformTransferDto transferDto = new(account1Id, account2Id, transferAmount);

            // Создаем HttpClient для выполнения переводов от имени user1
            using HttpClient transferClient = new();
            transferClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                apiClientUser1.DefaultRequestHeaders.Authorization!.Parameter);
            transferClient.BaseAddress = _apiHttpClient.BaseAddress;

            // Запуск параллельных переводов
            output.WriteLine($"Запуск {numberOfTransfers} последовательных переводов по {transferAmount} RUB...");
            DateTime startTime = DateTime.UtcNow;
            
            List<HttpResponseMessage?> responses = [];

            for (int i = 0; i < numberOfTransfers; i++)
            {
                // Создаем локальную копию для захвата в замыкании
                HttpClient clientCopy = transferClient;
                PerformTransferDto dtoCopy = transferDto;
                HttpResponseMessage? response = null;
                try
                {
                    // Выполняем перевод
                    response = await clientCopy.PostAsJsonAsync("/api/accounts/transfer", dtoCopy); 
                }
                catch (Exception ex)
                {
                    output.WriteLine($"Ошибка в задаче перевода: {ex}"); 
                }
                responses.Add(response);
                
            }

            // Ожидание завершения всех переводов
           
            DateTime endTime = DateTime.UtcNow;
            output.WriteLine($"Все переводы завершены за {endTime - startTime}");

            // Анализ результатов
            int successfulTransfers = 0;
            int failedTransfers = 0;
            int concurrencyConflicts = 0; // Счетчик конфликтов оптимистичной блокировки

            foreach (HttpResponseMessage? response in responses)
            {
                if (response == null)
                {
                    failedTransfers++;
                    output?.WriteLine("Задача перевода завершилась с исключением.");
                    continue;
                }

                if (response.IsSuccessStatusCode)
                {
                    successfulTransfers++;
                }
                else
                {
                    failedTransfers++;
                    string content = await response.Content.ReadAsStringAsync();
                    output?.WriteLine($"Неуспешный перевод. Статус: {response.StatusCode}, Content: {content}");

                    // Проверим, была ли ошибка связана с оптимистичной блокировкой
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        concurrencyConflicts++;
                    }
                }
            }

            output?.WriteLine($"Успешных переводов: {successfulTransfers}");
            output?.WriteLine($"Неуспешных переводов: {failedTransfers} (из них конфликтов: {concurrencyConflicts})");

            // Проверка суммарного баланса
            decimal finalBalanceUser1 = await ApiClientHelper.GetBalance(apiClientUser1, account1Id);
            decimal finalBalanceUser2 = await ApiClientHelper.GetBalance(apiClientUser2, account2Id);

            decimal totalFinalBalance = finalBalanceUser1 + finalBalanceUser2;

            output?.WriteLine($"Начальный суммарный баланс: {totalInitialBalance} RUB");
            output?.WriteLine($"Финальный суммарный баланс: {totalFinalBalance} RUB");
            output?.WriteLine($"Баланс счета 1: {finalBalanceUser1} RUB");
            output?.WriteLine($"Баланс счета 2: {finalBalanceUser2} RUB");
            
            await Task.Delay(10500); // ждем следующей публикации сообщений

           (string, string) logs = await _bankAccountsApiContainer.GetLogsAsync();
            string stdout = logs.Item1;
            int matchCount = TransferCompletedReg().Matches(stdout).Count;
            
            output?.WriteLine($"Количество сообщений о завершенных трансферах: {matchCount}");

            // Assert: Проверяем, что суммарный баланс сохранился
            Assert.Equal(totalInitialBalance, totalFinalBalance);

            // Проверяем, что общее количество попыток (успешные + неуспешные) равно количеству задач
            Assert.Equal(numberOfTransfers, successfulTransfers + failedTransfers);
            Assert.Equal(numberOfTransfers, matchCount);
        }
        
        [GeneratedRegex("Successfully consumed event.+TransferCompleted")]
        private static partial Regex TransferCompletedReg();
    }
    
    
}
