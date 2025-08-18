using BankAccounts.Api.Common;
using BankAccounts.Api.Infrastructure.Database.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Net;

// ReSharper disable UnusedMember.Global Методы используются

namespace BankAccounts.Api.Features.Healthcheck
{
    /// <summary>
    /// Контроллер проверок работы сервиса
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="configuration"></param>
    [ApiController]
    [Route("api/health/[action]")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public class HealthcheckController(IBankAccountsDbContext dbContext, IConfiguration configuration)
    {
        /// <summary>
        /// Проверка, работает ли сервис
        /// </summary>
        /// <returns>"Running", если работает</returns>
        [HttpGet]
        public async Task<MbResult<string>> Health()
        {
            MbResult<string> result = MbResult<string>.Success((int)HttpStatusCode.NoContent, "running");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Проверка на готовность обрабатывать запросы
        /// </summary>
        /// <returns>Список проверенных элементов и их состояние</returns>
        [HttpGet]
        [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status200OK)]
        public async Task<MbResult<object>> Ready()
        {
            string database = "not checked";
            string load = "not checked";
            string rabbitmq = "not checked";
            
            try
            {
                database = await CheckDb();
                rabbitmq = await CheckRabbit();
                load = CheckLoad();
                
                return MbResult<object>.Success((int)HttpStatusCode.OK, new {
                        status = "ready",
                        database,
                        rabbitmq,
                        load
                    });
            }
            catch (Exception ex)
            {
                return MbResult<object>.Failure((int)HttpStatusCode.ServiceUnavailable, "service unavailable", new { 
                    status = "unavailable",
                    database, 
                    rabbitmq,
                    load,
                    message = ex.Message 
                });
            }
        }

        private string CheckLoad()
        {
            int outboxCount = dbContext.OutboxPublished.Count();
            return outboxCount > 100 ? $"load warning, more than 100 outbox entries detected ({outboxCount})" : "ok";
        }

        private async Task<string> CheckDb()
        {
            // Выполняем простой SQL-запрос
            await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
            return "ready";
        }

        private async Task<string> CheckRabbit()
        {
            ConnectionFactory factory = new()
            {
                HostName = configuration["RabbitMQ:Hostname"]!, 
                UserName = configuration["RabbitMQ:Username"]!, 
                Password = configuration["RabbitMQ:Password"]!,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                SocketReadTimeout = TimeSpan.FromSeconds(5),
                SocketWriteTimeout = TimeSpan.FromSeconds(5)
            };

            await using IConnection connection = await factory.CreateConnectionAsync();
            await using IChannel channel = await connection.CreateChannelAsync();
            
            // Пытаемся объявить временную очередь
            await channel.QueueDeclareAsync("readiness_check");

            return "ready";
        }
    }
}
