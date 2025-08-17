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
using Testcontainers.RabbitMq;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BankAccounts.Tests.Integration.Testcontainers;
/// <summary>
/// Набор интеграционных тестов. Необходимо собрать образы сервисов BankAccounts API и BankAccounts Identity
/// перед выполнением тестов. Чтобы это сделать, запустите конфигурацию docker-compose или выполните команду
/// "docker-compose build" в корневой папке решения.
/// В случае ошибок:
/// 1. Отключите VPN (502 BadGateway)
/// 2. Выполните команду <code>docker-compose down -v</code> (Docker container conflict)
/// </summary>
/// <param name="output">Вспомогательный объект для вывода логов теста</param>
public class CrossServiceIntegrationTests(ITestOutputHelper output) : IAsyncLifetime
{
    // Используйте этот флаг для того чтобы не выключать контейнеры после тестов
    // (можно, например, посмотреть логи контейнеров в приложении Docker Desktop)
    private const bool CleanUp = true;
    
    private INetwork _network = null!; 
    private PostgreSqlContainer _bankAccountsDbContainer = null!;
    private IContainer _identityServiceContainer = null!; 
    private IContainer _bankAccountsApiContainer = null!; 
    private RabbitMqContainer _rabbitMqContainer = null!;
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
            .WithName("bankaccounts_db_test" + Random.Shared.NextInt64())
            .WithDatabase("bank-accounts-db")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithPortBinding(5432, true)
            .WithImage("lithiumkgp/postgres")
            .WithCleanUp(CleanUp)
            .Build();
        await _bankAccountsDbContainer.StartAsync();
        
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithNetwork(_network) 
            .WithImage("rabbitmq:4-management")
            .WithName("rabbitmq_test" + Random.Shared.NextInt64())
            .WithUsername("admin")
            .WithPassword("admin")
            .WithHostname("rabbitmq")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true) 
            .WithCleanUp(CleanUp)
            .Build();
        await _rabbitMqContainer.StartAsync();
        
        // Запуск контейнеров сервисов (предполагается, что образы собраны и доступны)
        _identityServiceContainer = new ContainerBuilder()
            .WithNetwork(_network) 
            .WithName("bankaccounts_identity_test" + Random.Shared.NextInt64())
            .WithImage("lithiumkgp/bankaccounts.identity:latest")
            .WithPortBinding(7045, true)
            .WithCleanUp(CleanUp)
            .Build();
        await _identityServiceContainer.StartAsync();

        _bankAccountsApiContainer = new ContainerBuilder()
            .WithNetwork(_network) 
            .WithName("bankaccounts_api_test" + Random.Shared.NextInt64())
            .WithImage("lithiumkgp/bankaccounts.api:latest") 
            .WithPortBinding(80, true)
            .WithEnvironment("ConnectionStrings__BankAccountsDbContext", DbConnectionString)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
            .DependsOn(_bankAccountsDbContainer)
            .DependsOn(_rabbitMqContainer)
            .WithCleanUp(CleanUp)
            .Build();
        await _bankAccountsApiContainer.StartAsync();

        // Настройка HttpClient для вызова API контейнера Identity
        ushort identityPort = _identityServiceContainer.GetMappedPublicPort(7045);
        _identityHttpClient = new HttpClient();
        _identityHttpClient.BaseAddress = new Uri($"http://localhost:{identityPort}/");
        
        // Настройка HttpClient для вызова API контейнера BankAccounts
        ushort bankApiPort = _bankAccountsApiContainer.GetMappedPublicPort(80);
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
        RegisterData registerData = new() { Username = "testuser", Password = "password" };
        LoginData loginData = new() { Username = "testuser", Password = "password" };

        // Регистрация пользователя через Identity контейнер
        ushort identityPort = _identityServiceContainer.GetMappedPublicPort(7045);
        
        using HttpClient identityClient = new();
        
        identityClient.BaseAddress = new Uri($"http://localhost:{identityPort}/ ");
        HttpResponseMessage registerResponse = await identityClient.PostAsJsonAsync("/api/auth/register", registerData);
        registerResponse.EnsureSuccessStatusCode();

        // Вход и получение токена через Identity контейнер
        HttpResponseMessage loginResponse = await identityClient.PostAsJsonAsync("/api/auth/login", loginData);
        loginResponse.EnsureSuccessStatusCode();
        string token = await loginResponse.Content.ReadAsStringAsync();
        _apiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim('"'));

        // Act
        HttpResponseMessage accountsResponse = await _apiHttpClient.GetAsync("/api/accounts/all");

        // Assert
        accountsResponse.EnsureSuccessStatusCode();
        string responseString = await accountsResponse.Content.ReadAsStringAsync();
        MbResult<List<AccountDto>>? result = JsonSerializer.Deserialize<MbResult<List<AccountDto>>>(responseString, GetJsonSerializerOptions());
        Assert.True(result!.IsSuccess);
    }
    
    [Fact]
    public async Task ParallelTransferTests_50ParallelTransfers_SumBalanceConserved()
    {
        using ApiClientHelper clientHelper = new(_identityHttpClient, _apiHttpClient);

        // Регистрация пользователей
        // Пользователь 1
        using HttpClient apiClientUser1 = await clientHelper.AuthorizeApiHttpClient("user1", "password");

        // Пользователь 2
        using HttpClient apiClientUser2 = await clientHelper.AuthorizeApiHttpClient("user1", "password");

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
        List<Task<HttpResponseMessage?>> tasks = []; // Список для хранения задач

        PerformTransferDto transferDto = new(account1Id, account2Id, transferAmount);

        // Создаем HttpClient для выполнения переводов от имени user1
        using HttpClient transferClient = new();
        transferClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
            apiClientUser1.DefaultRequestHeaders.Authorization!.Parameter);
        transferClient.BaseAddress = _apiHttpClient.BaseAddress;

        // Запуск параллельных переводов
        output.WriteLine($"Запуск {numberOfTransfers} параллельных переводов по {transferAmount} RUB...");
        DateTime startTime = DateTime.UtcNow;

        for (int i = 0; i < numberOfTransfers; i++)
        {
            // Создаем локальную копию для захвата в замыкании
            HttpClient clientCopy = transferClient;
            PerformTransferDto dtoCopy = transferDto;
            Task<HttpResponseMessage?> task = Task.Run(async () =>
            {
                try
                {
                    // Выполняем перевод
                    HttpResponseMessage response = await clientCopy.PostAsJsonAsync("/api/accounts/transfer", dtoCopy);
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
        HttpResponseMessage?[] responses = await Task.WhenAll(tasks);
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
            RegisterData userRegisterData = new() { Username = username, Password = password };
            LoginData userLoginData = new() { Username = username, Password = password };
            await identityHttpClient.PostAsJsonAsync("/api/auth/register", userRegisterData);
            HttpResponseMessage userLoginResponse = await identityHttpClient.PostAsJsonAsync("/api/auth/login", userLoginData);
            userLoginResponse.EnsureSuccessStatusCode();
            string? userTokenResponse = await userLoginResponse.Content.ReadAsStringAsync();
            string userToken = userTokenResponse ?? throw new InvalidOperationException($"Не удалось получить токен для {username}");
            
            HttpClient apiClientUser1 = new();
            apiClientUser1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            apiClientUser1.BaseAddress = apiHttpClient.BaseAddress; // Используем адрес BankAccountsAPI

            return apiClientUser1;
        }
        
        /// <summary>
        /// Создает счет для определенного пользователя
        /// </summary>
        internal static async Task<int> CreateAccount(HttpClient apiClientUser)
        {
            CreateAccountDto createAccountDto1 = new(AccountType.Checking, Currencies.Rub, 0);
            HttpResponseMessage createAccountResponse1 = await apiClientUser.PostAsJsonAsync("/api/accounts", createAccountDto1);
            createAccountResponse1.EnsureSuccessStatusCode();
            MbResult<AccountDto>? account1Result = await createAccountResponse1.Content.ReadFromJsonAsync<MbResult<AccountDto>>(GetJsonSerializerOptions());
            int account1Id = account1Result?.Value?.AccountId ?? throw new InvalidOperationException($"Не удалось создать счет для {apiClientUser}");
            return account1Id;
        }
        /// <summary>
        /// Вернет баланс определенного счета клиента
        /// </summary>
        /// <param name="apiClientUser"></param>
        /// <param name="accountId"></param>
        /// <returns>Баланс счета</returns>
        internal static async Task<decimal> GetBalance(HttpClient apiClientUser, int accountId)
        {
            HttpResponseMessage account1DetailsResponse = await apiClientUser.GetAsync($"/api/accounts/{accountId}");
            account1DetailsResponse.EnsureSuccessStatusCode();
            MbResult<AccountDto>? account1Details = await account1DetailsResponse.Content.ReadFromJsonAsync<MbResult<AccountDto>>(GetJsonSerializerOptions());
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
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        return options;
    }

    public async Task DisposeAsync()
    {
#pragma warning disable CS0162 // Unreachable code detected
        if (!CleanUp)
            // ReSharper disable once HeuristicUnreachableCode Оставлено в целях дальнейшего использования при тестах

            return;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract Возможен null в исключительном случае
        _apiHttpClient?.Dispose();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_bankAccountsApiContainer != null) await _bankAccountsApiContainer.DisposeAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_identityServiceContainer != null) await _identityServiceContainer.DisposeAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_bankAccountsDbContainer != null) await _bankAccountsDbContainer.DisposeAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_rabbitMqContainer != null) await _rabbitMqContainer.DisposeAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Возможен null в исключительном случае
        if (_network != null) await _network.DeleteAsync();
#pragma warning restore CS0162 // Unreachable code detected
    }
}