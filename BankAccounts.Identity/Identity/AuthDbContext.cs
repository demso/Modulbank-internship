using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Identity.Identity;

/// <summary>
/// Контекст базы данных
/// </summary>
/// <param name="options"></param>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<BankUser>(options)
{
    /// <summary>
    /// Конфигурация контекста
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("AuthenticationDatabase");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new BankUserConfiguration());
    }
}
