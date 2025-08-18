using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Specific;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace BankAccounts.Api.Infrastructure;

/// <summary>
/// Класс для конфигурации Swagger
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Метод конфигурации
    /// </summary>
    /// <param name="options"></param>
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Bank Accounts API",
            Description = "Для авторизации воспользуйтесь [http://localhost:7045](http://localhost:7045)\n" +
                          "1. Зарегистрируйтесь с указанием логина и пароля (Register).\n" +
                          "2. Войдите, также указав логин и пароль (Login).\n" +
                          "3. Скопируйте полученный токен.\n" +
                          "4. Вставьте в поле окна \"Authorize\"\n" +
                          "5. Можно пользоваться сервисом.\n\n\n" +
                          "Hangfire Dashboard: [http://localhost:80/hangfire](http://localhost:80/hangfire)\n"+
                          "RabbitMQ: [http://localhost:15672/](http://localhost:15672/)" ,
            Version = "v1"
        });

        string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.AddSecurityDefinition("AuthToken",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer",
                Name = "Authorization",
                Description = "Authorization token"
            });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "AuthToken"
                    }
                },
                []
            }
        });

        options.CustomOperationIds(apiDescription =>
            apiDescription.TryGetMethodInfo(out MethodInfo? methodInfo)
                ? methodInfo.Name
                : null);
        options.DocumentFilter<EventTypeSchemasDocumentFilter>();
    }
    
    /// <summary>
    /// Фильтр для добавления пользовательских описаний к схемам событий.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global Предлагает ерунду
    public class EventTypeSchemasDocumentFilter : IDocumentFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            context.SchemaGenerator.GenerateSchema(typeof(AccountOpened), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(TransferCompleted), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(ClientBlocked), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(ClientUnblocked), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(MoneyCredited), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(MoneyDebited), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(Metadata), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(InterestAccrued), context.SchemaRepository);
        }
    }
}
