using System.Reflection;
using AutoMapper;
using BankAccounts.Api.Infrastructure;
using BankAccounts.Api.Mapping;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services
        .AddDbContext<BankAccountsContext>()
        .AddScoped<IBankAccountsContext>(provider =>
            provider.GetRequiredService<BankAccountsContext>()!)
        .AddAutoMapper(options => options
            .AddProfile(new AssemblyMappingProfile(Assembly.GetExecutingAssembly())))
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
