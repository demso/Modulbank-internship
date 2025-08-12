using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Accounts.Dtos;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Features.Transactions.Dtos;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Identity.Identity;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BankAccounts.Tests.Integration.Testcontainers;
/// <summary>
/// Набор интеграционных тестов. Необходимо собрать образы сервисов BankAccounts.Api и BankAccounts.Identity
/// перед выполнением тестов. Чтобы это сделать, запустите конфигурацию docker-compose или выполните команду
/// "docker-compose build" в корневой папке решения.
/// </summary>
/// <param name="output">Вспомогательный объект для вывода логов теста</param>
public class CrossServiceIntegrationTests(ITestOutputHelper output) : IAsyncLifetime
{
    private INetwork _network = null!; 
    private PostgreSqlContainer _bankAccountsDbContainer = null!;
    private IContainer _identityServiceContainer = null!; 
    private IContainer _bankAccountsApiContainer = null!; 
    private HttpClient _apiHttpClient = null!;
    private HttpClient _identityHttpClient = null!;

    private const string DbConnectionString =
        "Host=bankaccounts.db;Port=5432;Database=bank-accounts-db;Username=postgres;Password=password";

    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder()
            .Build();
        await _network.CreateAsync();
        // Запуск контейнера базы данных
        _bankAccountsDbContainer = new PostgreSqlBuilder()
            .WithNetwork(_network) 
            .WithNetworkAliases("bankaccounts.db")
            .WithName("bankaccounts_db_test")
            .WithDatabase("bank-accounts-db")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithPortBinding(5432, true)
            .WithImage("lithiumkgp/postgres:latest")
            .Build();
        await _bankAccountsDbContainer.StartAsync();
        
        // Запуск контейнеров сервисов (предполагается, что образы собраны и доступны)
        _identityServiceContainer = new ContainerBuilder()
            .WithNetwork(_network) 
            .WithName("bankaccounts_identity_test")
            .WithImage("lithiumkgp/bankaccounts.identity:latest")
            .WithPortBinding(7045, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(7045))
            .Build();
        await _identityServiceContainer.StartAsync();

        _bankAccountsApiContainer = new ContainerBuilder()
            .WithNetwork(_network) 
            .WithName("bankaccounts_api_test")
            .WithImage("lithiumkgp/bankaccounts.api:latest") 
            .WithPortBinding(80, true)
            .WithEnvironment("ConnectionStrings__BankAccountsDbContext", DbConnectionString)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
            .DependsOn(_bankAccountsDbContainer)
            .Build();
        await _bankAccountsApiContainer.StartAsync();

        // Настройка HttpClient для вызова API контейнера Identity
        var identityPort = _identityServiceContainer.GetMappedPublicPort(7045);
        _identityHttpClient = new HttpClient();
        _identityHttpClient.BaseAddress = new Uri($"http://localhost:{identityPort}/");
        
        // Настройка HttpClient для вызова API контейнера BankAccounts
        var bankApiPort = _bankAccountsApiContainer.GetMappedPublicPort(80);
        _apiHttpClient = new HttpClient();
        _apiHttpClient.BaseAddress = new Uri($"http://localhost:{bankApiPort}/");
    }

    /// <summary>
    /// Интеграционный тест. Проверяет, успешно ли выполняется запрос всех счетов пользователя  
    /// </summary>
    [Fact]
    public async Task GetUserAccounts_WithValidTokenFromIdentity_ReturnsAccounts()
    {
        // Arrange
        var registerData = new RegisterData { Username = "testuser", Password = "password" };
        var loginData = new LoginData { Username = "testuser", Password = "password" };

        // Регистрация пользователя через Identity контейнер
        var identityPort = _identityServiceContainer.GetMappedPublicPort(7045);
        
        using var identityClient = new HttpClient();
        
        identityClient.BaseAddress = new Uri($"http://localhost:{identityPort}/ ");
        var registerResponse = await identityClient.PostAsJsonAsync("/api/auth/register", registerData);
        registerResponse.EnsureSuccessStatusCode();

        // Вход и получение токена через Identity контейнер
        var loginResponse = await identityClient.PostAsJsonAsync("/api/auth/login", loginData);
        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadAsStringAsync();
        _apiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim('"'));

        // Act
        var accountsResponse = await _apiHttpClient.GetAsync("/api/accounts/all");

        // Assert
        accountsResponse.EnsureSuccessStatusCode();
        var responseString = await accountsResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MbResult<List<AccountDto>>>(responseString, GetJsonSerializerOptions());
        Assert.True(result!.IsSuccess);
    }
    
    [Fact]
    public async Task ParallelTransferTests_50ParallelTransfers_SumBalanceConserved()
    {
        using var clientHelper = new ApiClientHelper(_identityHttpClient, _apiHttpClient);

        // Регистрация пользователей
        // Пользователь 1
        using var apiClientUser1 = await clientHelper.AuthorizeApiHttpClient("user1", "password");

        // Пользователь 2
        using var apiClientUser2 = await clientHelper.AuthorizeApiHttpClient("user1", "password");

        // Создание счетов
        var account1Id = await clientHelper.CreateAccount(apiClientUser1);
        var account2Id = await clientHelper.CreateAccount(apiClientUser2); 
        
        // Пополнение счетов начальными средствами
        var initialBalanceUser1 = 10000m; 
        var initialBalanceUser2 = 5000m; 
        var totalInitialBalance = initialBalanceUser1 + initialBalanceUser2;

        // Пополнение счета 1
        var depositDto1 = new PerformTransactionDto(TransactionType.Debit, initialBalanceUser1, "Начальное пополнение");
        await apiClientUser1.PostAsJsonAsync($"/api/accounts/{account1Id}/transactions", depositDto1);

        // Пополнение счета 2
        var depositDto2 = new PerformTransactionDto(TransactionType.Debit, initialBalanceUser2, "Начальное пополнение");
        await apiClientUser2.PostAsJsonAsync($"/api/accounts/{account2Id}/transactions", depositDto2);

        // Проверка начальных балансов
        Assert.Equal(initialBalanceUser1, await clientHelper.GetBalance(apiClientUser1, account1Id));
        Assert.Equal(initialBalanceUser2, await clientHelper.GetBalance(apiClientUser2, account2Id));

        // Настройка параллельных переводов
        var transferAmount = 10m; // Сумма одного перевода
        var numberOfTransfers = 50; // Количество параллельных переводов
        var fromAccountId = account1Id; // Переводим со счета user1
        var toAccountId = account2Id;    // На счет user2
        var tasks = new List<Task<HttpResponseMessage?>>(); // Список для хранения задач

        var transferDto = new PerformTransferDto(fromAccountId, toAccountId, transferAmount);

        // Создаем HttpClient для выполнения переводов от имени user1
        using var transferClient = new HttpClient();
        transferClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
            apiClientUser1.DefaultRequestHeaders.Authorization!.Parameter);
        transferClient.BaseAddress = _apiHttpClient.BaseAddress;

        // Запуск параллельных переводов
        output.WriteLine($"Запуск {numberOfTransfers} параллельных переводов по {transferAmount} RUB...");
        var startTime = DateTime.UtcNow;

        for (int i = 0; i < numberOfTransfers; i++)
        {
            // Создаем локальную копию для захвата в замыкании
            var clientCopy = transferClient;
            var dtoCopy = transferDto;
            var task = Task.Run(async () =>
            {
                try
                {
                    // Выполняем перевод
                    var response = await clientCopy.PostAsJsonAsync("/api/accounts/transfer", dtoCopy);
                    return response; 
                }
                catch (Exception ex)
                {
                    output.WriteLine($"Ошибка в задаче перевода: {ex}");
                    return null; 
                }
            });
            tasks.Add(task);
        }

        // Ожидание завершения всех переводов
        var responses = await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        output.WriteLine($"Все переводы завершены за {endTime - startTime}");

        // Анализ результатов
        var successfulTransfers = 0;
        var failedTransfers = 0;
        var concurrencyConflicts = 0; // Счетчик конфликтов оптимистичной блокировки

        foreach (var response in responses)
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
                var content = await response.Content.ReadAsStringAsync();
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
        var finalBalanceUser1 = await clientHelper.GetBalance(apiClientUser1, account1Id);
        var finalBalanceUser2 = await clientHelper.GetBalance(apiClientUser2, account2Id);

        var totalFinalBalance = finalBalanceUser1 + finalBalanceUser2;

        output?.WriteLine($"Начальный суммарный баланс: {totalInitialBalance} RUB");
        output?.WriteLine($"Финальный суммарный баланс: {totalFinalBalance} RUB");
        output?.WriteLine($"Баланс счета 1: {finalBalanceUser1} RUB");
        output?.WriteLine($"Баланс счета 2: {finalBalanceUser2} RUB");

        // Assert: Проверяем, что суммарный баланс сохранился
        Assert.Equal(totalInitialBalance, totalFinalBalance);

        // Проверяем, что общее количество попыток (успешные + неуспешные) равно количеству задач
        Assert.Equal(numberOfTransfers, successfulTransfers + failedTransfers);
    }
    
    /// <summary>
    /// Вспомогательный класс для теста 50-ти параллельных переводов
    /// </summary>
    /// <param name="identityHttpClient"></param>
    /// <param name="apiHttpClient"></param>
    private class ApiClientHelper(HttpClient identityHttpClient, HttpClient apiHttpClient) : IDisposable
    {
        /// <summary>
        /// Регистрирует, авторизует и добавляет токен в заголовок созданного HttpClient'a и возвращает его.
        /// </summary>
        internal async Task<HttpClient> AuthorizeApiHttpClient(string username, string password)
        {
            var userRegisterData = new RegisterData { Username = username, Password = password };
            var userLoginData = new LoginData { Username = username, Password = password };
            await identityHttpClient.PostAsJsonAsync("/api/auth/register", userRegisterData);
            var userLoginResponse = await identityHttpClient.PostAsJsonAsync("/api/auth/login", userLoginData);
            userLoginResponse.EnsureSuccessStatusCode();
            var userTokenResponse = await userLoginResponse.Content.ReadAsStringAsync();
            var userToken = userTokenResponse ?? throw new InvalidOperationException($"Не удалось получить токен для {username}");
            
            var apiClientUser1 = new HttpClient();
            apiClientUser1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            apiClientUser1.BaseAddress = apiHttpClient.BaseAddress; // Используем адрес BankAccountsAPI

            return apiClientUser1;
        }
        
        /// <summary>
        /// Создает счет для определенного пользователя
        /// </summary>
        internal async Task<int> CreateAccount(HttpClient apiClientUser)
        {
            var createAccountDto1 = new CreateAccountDto (AccountType.Checking, Currencies.Rub, 0);
            var createAccountResponse1 = await apiClientUser.PostAsJsonAsync("/api/accounts", createAccountDto1);
            createAccountResponse1.EnsureSuccessStatusCode();
            var account1Result = await createAccountResponse1.Content.ReadFromJsonAsync<MbResult<AccountDto>>(GetJsonSerializerOptions());
            var account1Id = account1Result?.Value?.AccountId ?? throw new InvalidOperationException($"Не удалось создать счет для {apiClientUser}");
            return account1Id;
        }
        /// <summary>
        /// Вернет баланс определенного счета клиента
        /// </summary>
        /// <param name="apiClientUser"></param>
        /// <param name="accountId"></param>
        /// <returns>Баланс счета</returns>
        internal async Task<decimal> GetBalance(HttpClient apiClientUser, int accountId)
        {
            var account1DetailsResponse = await apiClientUser.GetAsync($"/api/accounts/{accountId}");
            account1DetailsResponse.EnsureSuccessStatusCode();
            var account1Details = await account1DetailsResponse.Content.ReadFromJsonAsync<MbResult<AccountDto>>(GetJsonSerializerOptions());
            return account1Details!.Value!.Balance;
        }
        public void Dispose()
        {
            identityHttpClient.Dispose();
            apiHttpClient.Dispose();
        }
    }
    
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        return options;
    }

    public async Task DisposeAsync()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract Возможен null в исключительном случае
        _apiHttpClient?.Dispose();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_bankAccountsApiContainer != null) await _bankAccountsApiContainer.DisposeAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_identityServiceContainer != null) await _identityServiceContainer.DisposeAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_bankAccountsDbContainer != null) await _bankAccountsDbContainer.DisposeAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_network != null) await _network.DeleteAsync();
    }
}