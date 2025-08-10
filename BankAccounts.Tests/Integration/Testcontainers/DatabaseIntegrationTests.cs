using Npgsql;
using Testcontainers.PostgreSql;

namespace BankAccounts.Tests.Integration.Testcontainers;

public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("lithiumkgp/postgres:latest")
        .WithDatabase("testdb")
        .WithUsername("user")
        .WithPassword("password")
        .Build();
    private string _connectionString = null!;

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
        await using  var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1", connection);
        
        // Act
        var result = await cmd.ExecuteScalarAsync();

        //Assert
        Assert.Equal(1, Convert.ToInt32(result));
    }
}
