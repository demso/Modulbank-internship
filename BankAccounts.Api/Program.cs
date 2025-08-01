using System.Reflection;
using System.Text.Json.Serialization;
using BankAccounts.Api;
using BankAccounts.Api.Features;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using IdentityServer4.Models;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<BankAccountsContext>()
    .AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly()])
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
    .AddScoped<IBankAccountsContext>(provider => provider.GetRequiredService<BankAccountsContext>()!)
    .AddAutoMapper(options => options.AddProfile(new MappingProfile()))
    .AddMediatR(options => options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddIdentityServer()
    .AddInMemoryApiResources(new List<ApiResource>())
    .AddInMemoryIdentityResources(new List<IdentityResource>())
    .AddInMemoryApiScopes(new List<ApiScope>())
    .AddInMemoryClients(new List<Client>())
    .AddDeveloperSigningCredential();

builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
}));

var app = builder.Build();


app.UseRouting();
app.UseIdentityServer();
app.UseCors("AllowAll");
app.UseMiddleware<CustomExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();
