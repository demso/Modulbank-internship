using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Api.Identity;

public class AuthDbContext : IdentityDbContext<BankUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("AuthenticationDatabase");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BankUser>(entity => entity.ToTable(name: "Users"));
        builder.Entity<IdentityUserClaim<string>>(entity =>
            entity.ToTable(name: "UserClaim"));
        builder.Entity<IdentityUserLogin<string>>(entity =>
            entity.ToTable("UserLogins"));
        builder.Entity<IdentityUserToken<string>>(entity =>
            entity.ToTable("UserTokens"));

        builder.ApplyConfiguration(new BankUserConfiguration());
    }
}