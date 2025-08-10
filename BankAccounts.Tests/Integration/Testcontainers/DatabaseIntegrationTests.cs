using Npgsql;
using Testcontainers.PostgreSql;

namespace BankAccounts.Tests.Testcontainers;

public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private string _connectionString;

    public DatabaseIntegrationTests()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres")
            .WithDatabase("testdb")
            .WithUsername("user")
            .WithPassword("password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    [Fact]
    public async Task Can_Connect_To_Database()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var cmd = new NpgsqlCommand("SELECT 1", connection);
        
        // Act
        var result = await cmd.ExecuteScalarAsync();

        //Assert
        Assert.Equal(1, Convert.ToInt32(result));
    }
}
