using System.Reflection;
using System.Text.Json.Serialization;
using BankAccounts.Api;
using BankAccounts.Api.Features;
using BankAccounts.Api.Infrastructure;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services
        .AddDbContext<BankAccountsContext>()
        .AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly()])
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
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

app.UseMiddleware<CustomExceptionHandlerMiddleware>();
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
