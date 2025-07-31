using System.Reflection;
using System.Text.Json.Serialization;
using BankAccounts.Api;
using BankAccounts.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services
        .AddDbContext<BankAccountsContext>()
        .AddScoped<IBankAccountsContext>(provider => provider.GetRequiredService<BankAccountsContext>()!)
        .AddAutoMapper(options => options.AddProfile(new MappingProfile()))
        .AddMediatR(options => options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
        .AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    
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
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
