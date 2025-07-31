using System.Reflection;
using System.Text.Json.Serialization;
using AutoMapper;
using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Infrastructure;
using BankAccounts.Api.Mapping;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services
        .Configure<JsonOptions>(options => options
            .SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        .AddDbContext<BankAccountsContext>()
        .AddScoped<IBankAccountsContext>(provider =>
            provider.GetRequiredService<BankAccountsContext>()!)
        .AddAutoMapper(options => options
            .AddProfile(new AccountMappingProfile()))
        .AddMediatR(options => options
            .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
        .AddControllers();
}
catch (Exception exception)
{
    Console.WriteLine(exception);
}

builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
}));

var app = builder.Build();

app.UseRouting();
//app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();

//var scope = app.Services.CreateScope();
//var dbContext = scope.ServiceProvider.GetRequiredService<BankAccountsContext>();
//dbContext.Database.EnsureCreated();

app.Run();
