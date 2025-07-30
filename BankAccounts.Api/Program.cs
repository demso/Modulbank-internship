using System.Reflection;
using BankAccounts.Api.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services
        .AddDbContext<BankAccountsContext>()
        .AddScoped<IBankAccountsContext>(provider =>
            provider.GetRequiredService<BankAccountsContext>()!)
        .AddMediatR(options => options
            .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
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
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();

//var scope = app.Services.CreateScope();
//var dbContext = scope.ServiceProvider.GetRequiredService<BankAccountsContext>();
//dbContext.Database.EnsureCreated();

app.Run();
