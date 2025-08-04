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
            Description = "Для авторизации воспользуйтесь [http://localhost:7045](http://localhost:7045). \n" +
                          "1. Зарегистрируйтесь с указанием логина и пароля (Register).\n" +
                          "2. Войдите, также указав логин и пароль (Login).\n" +
                          "3. Скопируйте полученный токен.\n" +
                          "4. Вставьте в поле окна \"Authorize\"\n" +
                          "5. Можно пользоваться сервисом.",

            Version = "v1"
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
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
            apiDescription.TryGetMethodInfo(out var methodInfo)
                ? methodInfo.Name
                : null);
    }
}
