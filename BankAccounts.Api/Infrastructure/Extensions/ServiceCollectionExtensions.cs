using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.Hangfire.Jobs;
using BankAccounts.Api.Infrastructure.Hangfire.Registrator;
using BankAccounts.Api.Infrastructure.RabbitMQ;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankAccounts.Api.Infrastructure.Extensions
{
    /// <summary>
    /// Набор методов расширения IServiceCollection для добавления и настройки необходимых компонентов для сервиса банковских счетов
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static JsonSerializerOptions JsonOptions = new();
        /// <summary>
        /// Добавит компоненты, не требующие сложной настройки
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCommonServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            JsonOptions.Converters.Add(new JsonStringEnumConverter());
            
            services
                .AddDbContext<BankAccountsDbContext>(optionsBuilder => 
                    optionsBuilder.UseNpgsql(configuration.GetConnectionString(nameof(BankAccountsDbContext)), options =>
                    {
                        options.MapEnum<Currencies>();
                        options.MapEnum<TransactionType>();
                        options.MapEnum<AccountType>();
                        options.MapEnum<EventType>();
                    })
                )
                .AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly()])
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
                .AddScoped<IBankAccountsDbContext>(provider => provider.GetRequiredService<BankAccountsDbContext>())
                .AddScoped<IAccountsRepositoryAsync, AccountsRepositoryAsync>()
                .AddScoped<ITransactionsRepositoryAsync, TransactionsRepositoryAsync>()
                .AddSingleton<ICurrencyService, CurrencyService.CurrencyService>()
                .AddAutoMapper(options => options.AddMaps(Assembly.GetExecutingAssembly()))
                .AddMediatR(options => options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            
            return services;
        }

        /// <summary>
        /// Добавит и настроит обработчик ModelState ошибок, чтоб возвращал <see cref="MbResult"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection SetupApiBehavior(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    KeyValuePair<string, ModelStateEntry?> error = context.ModelState
                        .First(x => x.Value?.Errors.Count > 0);

                    MbResult<object?> result = MbResult.Failure((int)HttpStatusCode.BadRequest, $"Validation error: {error.Value?.Errors[0].ErrorMessage} {error.Value?.Errors[0].Exception?.Message}");

                    return new ObjectResult(result);
                };
            });
            return services;
        }

        /// <summary>
        /// Настроит Cors
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection SetupCors(this IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowAnyOrigin();
            }));

            return services;
        }

        /// <summary>
        /// Добавит и настроит аутентификацию по токену 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection SetupAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(config =>
                {
                    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Authority = "http://localhost:7045";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                        )
                    };
                });
            
            return services;
        }

        /// <summary>
        /// Добавит и настроит все необходимые компоненты Hangfire
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection SetupHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config => config.UsePostgreSqlStorage(c =>
                    c.UseNpgsqlConnection(configuration.GetConnectionString("BankAccountsDbContext"))))
                .AddHangfireServer()
                .AddHostedService<JobsRegistrator>();
            
            return services;
        }

        /// <summary>
        /// Добавит и настроит необходимые компоненты Swagger
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection SetupSwagger(this IServiceCollection services)
        {
            services
                .AddEndpointsApiExplorer()
                .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
                .AddSwaggerGen();

            return services;
        }

        /// <summary>
        /// Добавит и настроит необходимые компоненты RabbitMQ
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection SetupRabbitMq(this IServiceCollection services)
        {
            services.AddScoped<Sender>();
            services.AddHostedService<Reciever>();
            return services;
        }

        /// <summary>
        /// Добавит и настроит Serilog
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection SetupSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSerilog((servicess, lc) => lc
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(servicess)
                .Enrich.FromLogContext()
                .WriteTo.Console(new ExpressionTemplate(
                    // Include trace and span ids when present.
                    "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {SourceContext} \n{@m}\n{@x}",
                    theme: TemplateTheme.Code)));

            return services;
        }
    }
}