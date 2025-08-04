using System.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BankAccounts.Identity;

/// <summary>
/// Настройка swagger
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Bank Accounts Authentication Server",
            Description = "Сервер аутентификации для сервиса банковских счетов.",
            Version = "v1"
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.CustomOperationIds(apiDescription =>
            apiDescription.TryGetMethodInfo(out var methodInfo)
                ? methodInfo.Name
                : null);
    }
}
