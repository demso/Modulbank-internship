using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace BankAccounts.Tests.Integration.Testcontainers.Utility.Factories
{
    public class ContainerFactory
    {
        private const bool CleanUp = true;
        
        private IConfiguration _configuration = null!;

        public INetwork Network { get; set; } = null!;

        public async Task<ContainerFactory> InitializeAndAndBuildAsync()
        {
            Network = new NetworkBuilder()
                .Build();
            await Network.CreateAsync();
            
            _configuration = BuildConfiguration("appsettings.Docker.json");
            
            return this;
        }

        private IConfiguration BuildConfiguration(string name)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables(); 
            
            return builder.Build();
        }

        public async Task<IContainer> BuildApiContainer(IContainer dbCont, IContainer rabbitCont)
        {
            return await BuildApiContainer(Network, dbCont, rabbitCont,  CleanUp);
        }
        
        private async Task<IContainer> BuildApiContainer(INetwork network, IContainer dbCont, IContainer rabbitCont, bool cleanUp)
        {
            IContainer container = new ContainerBuilder()
                .WithNetwork(network) 
                .WithName("bankaccounts_api_test" + Random.Shared.NextInt64())
                .WithImage("lithiumkgp/bankaccounts.api:latest") 
                .WithPortBinding(80, true)
                .WithEnvironment("ConnectionStrings__BankAccountsDbContext", _configuration["ConnectionStrings:BankAccountsDbContext"])
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(dbCont)
                .DependsOn(rabbitCont)
                .WithCleanUp(CleanUp)
                .Build();
            await container.StartAsync();
            return container;
        }

        public async Task<PostgreSqlContainer> BuildPostgresContainer()
        {
            return await BuildPostgresContainer(Network, CleanUp);
        }

        private async Task<PostgreSqlContainer> BuildPostgresContainer(INetwork network, bool cleanUp)
        {
            PostgreSqlContainer container = new PostgreSqlBuilder()
                .WithNetwork(network) 
                .WithNetworkAliases("bankaccounts.db")
                .WithName("bankaccounts_db_test" + Random.Shared.NextInt64())
                .WithDatabase("bank-accounts-db")
                .WithUsername("postgres")
                .WithPassword("password")
                .WithPortBinding(5432, true)
                .WithImage("lithiumkgp/postgres")
                .WithCleanUp(cleanUp)
                .Build();
            await container.StartAsync();
            return container;
        }

        public async Task<RabbitMqContainer> BuildRabbitMqContainer()
        {
            return await BuildRabbitMqContainer(Network, CleanUp);
        }

        private async Task<RabbitMqContainer> BuildRabbitMqContainer(INetwork network, bool cleanUp)
        {
            RabbitMqContainer container = new RabbitMqBuilder()
                .WithNetwork(network) 
                .WithImage("lithiumkgp/rabbitmq:latest")
                .WithName("rabbitmq_test" + Random.Shared.NextInt64())
                .WithUsername("admin")
                .WithPassword("admin")
                .WithHostname("rabbitmq")
                .WithPortBinding(5672, true)
                .WithPortBinding(15672, true) 
                .WithCleanUp(cleanUp)
                .Build();
            await container.StartAsync();
            return container;
        }

        public async Task<IContainer> BuildIdentityContainer()
        {
            return await BuildIdentityContainer(Network, CleanUp);
        }

        private async Task<IContainer> BuildIdentityContainer(INetwork network, bool cleanUp)
        {
            IContainer container = new ContainerBuilder()
                .WithNetwork(network) 
                .WithName("bankaccounts_identity_test" + Random.Shared.NextInt64())
                .WithImage("lithiumkgp/bankaccounts.identity:latest")
                .WithPortBinding(7045, true)
                .WithCleanUp(cleanUp)
                .Build();
            await container.StartAsync();
            return container;
        }
    }
}
