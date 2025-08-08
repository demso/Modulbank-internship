using BankAccounts.Api.Features.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.Database.EntityTypeConfiguration;
/// <summary>
/// Конфигурация сущности счета
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.AccountId);

        builder.HasIndex(a => a.OwnerId)
            .HasMethod("hash");
        builder.Property(a => a.OpenDate).IsRequired();

        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId);
        // поле для оптимистичной блокировки
        builder.Property(a => a.Version)
            .IsRowVersion();
    }
}