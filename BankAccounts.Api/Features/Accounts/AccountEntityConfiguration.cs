using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Features.Accounts;
/// <summary>
/// Конфигурация сущности счета <see cref="Account"/>
/// </summary>
public class AccountEntityConfiguration : IEntityTypeConfiguration<Account>
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