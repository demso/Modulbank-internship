using BankAccounts.Api.Infrastructure.Database.Migrator;
using BankAccounts.Api.Infrastructure.Extensions;
using BankAccounts.Api.Infrastructure.Hangfire;
using BankAccounts.Api.Middleware;
using Hangfire;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;

services
    .AddCommonServices(builder.Configuration)
    .SetupApiBehavior()
    .SetupCors()
    .SetupAuthentication(builder.Configuration)
    .SetupHangfire(builder.Configuration)
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

app.Run();

