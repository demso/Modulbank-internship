using BankAccounts.Api.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using BankAccounts.Api.Infrastructure.CurrencyService;
using BankAccounts.Api.Infrastructure;
using BankAccounts.Api.Infrastructure.Database;
using BankAccounts.Api.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<BankAccountsDbContext>()
    .AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly()])
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
    .AddScoped<IBankAccountsDbContext>(provider => provider.GetRequiredService<BankAccountsDbContext>())
    .AddTransient<ICurrencyService, CurrencyService>()
    .AddAutoMapper(options => options.AddProfile(new MappingProfile()))
    .AddMediatR(options => options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
}));

builder.Services.AddAuthentication(config =>
    {
        config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:7044";
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

builder.Services
    .AddEndpointsApiExplorer()
    .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
    .AddSwaggerGen();

var app = builder.Build();

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
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

