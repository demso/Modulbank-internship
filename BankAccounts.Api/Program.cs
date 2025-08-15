using BankAccounts.Api.Infrastructure.Database.Migrator;
using BankAccounts.Api.Infrastructure.Extensions;
using BankAccounts.Api.Infrastructure.Hangfire;
using BankAccounts.Api.Middleware;
using Hangfire;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up!");

try
{ 
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    IServiceCollection services = builder.Services;
    IConfiguration configuration = builder.Configuration;

    services
        .SetupSerilog(configuration)
        .AddCommonServices(configuration)
        .SetupApiBehavior()
        .SetupCors()
        .SetupAuthentication(configuration)
        .SetupHangfire(configuration)
        .SetupSwagger()
        .SetupRabbitMq();

    WebApplication app = builder.Build();

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

    await app.RunAsync();

    Log.Information("Stopped cleanly");
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}


