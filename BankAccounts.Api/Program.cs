using BankAccounts.Api.Common;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.Database.Migrator;
using BankAccounts.Api.Infrastructure.Hangfire;
using BankAccounts.Api.Infrastructure.Hangfire.Registrator;
using BankAccounts.Api.Infrastructure.Repository.Accounts;
using BankAccounts.Api.Infrastructure.Repository.Transactions;
using BankAccounts.Api.Middleware;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

// Common services
services
    .AddDbContext<BankAccountsDbContext>(optionsBuilder => 
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString(nameof(BankAccountsDbContext)), options =>
        {
            options.MapEnum<Currencies>();
            options.MapEnum<TransactionType>();
            options.MapEnum<AccountType>();
        })
    )
    .AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly()])
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
    .AddScoped<IBankAccountsDbContext>(provider => provider.GetRequiredService<BankAccountsDbContext>())
    .AddScoped<IAccountsRepositoryAsync, AccountsRepositoryAsync>()
    .AddScoped<ITransactionsRepositoryAsync, TransactionsRepositoryAsync>()
    .AddSingleton<ICurrencyService, CurrencyService>()
    .AddAutoMapper(options => options.AddProfile(new MappingProfile()))
    .AddMediatR(options => options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Cors
services.AddCors(options => options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
}));

// Authentication
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// Hangfire
services.AddHangfire(config => config.UseInMemoryStorage())
    .AddHangfireServer()
    .AddHostedService<JobsRegistrator>();

// Swagger
services
    .AddEndpointsApiExplorer()
    .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
    .AddSwaggerGen();

var app = builder.Build();

await app.MigrateDatabase();

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(config =>
{
    config.InjectStylesheet("swagger-ui/custom.css");
    config.RoutePrefix = string.Empty;
    config.SwaggerEndpoint("swagger/v1/swagger.json", "BankAccounts API");
});

app.UseMiddleware<CustomExceptionHandlerMiddleware>();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});

app.Run();

