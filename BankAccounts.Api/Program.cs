using BankAccounts.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<BankAccountsContext>();
dbContext.Database.EnsureCreated();

app.Run();
