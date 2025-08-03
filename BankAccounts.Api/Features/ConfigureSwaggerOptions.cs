using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace BankAccounts.Api.Features;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "My API",
            Version = "v1"
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.AddSecurityDefinition($"AuthToken",
            new OpenApiSecurityScheme
            {
                Name = "session",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Cookie,
                Description = "Session cookie authentication",
                Scheme = "cookie"
                //In = ParameterLocation.Header,
                //Type = SecuritySchemeType.Http,
                //BearerFormat = "JWT",
                //Scheme = "bearer",
                //Name = "Authorization",
                //Description = "Authorization token"
            });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Cookies"
                        }
                    },
                    []
                    //new OpenApiSecurityScheme
                    //{
                    //    Reference = new OpenApiReference
                    //    {
                    //        Type = ReferenceType.SecurityScheme,
                    //        Id = $"AuthToken"
                    //    }
                    //},
                    //[]
                }
            });

        options.CustomOperationIds(apiDescription =>
            apiDescription.TryGetMethodInfo(out var methodInfo)
                ? methodInfo.Name
                : null);
    }
}
