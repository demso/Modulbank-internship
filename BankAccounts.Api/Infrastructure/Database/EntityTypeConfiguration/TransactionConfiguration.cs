using BankAccounts.Api.Features.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.Database.EntityTypeConfiguration
{
    /// <summary>
    /// Конфигурация сущности транзакции
    /// </summary>
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.TransactionId);

            builder.Property(t => t.AccountId).IsRequired();
            builder.Property(t => t.DateTime).IsRequired();
            builder.Property(t => t.Description).HasMaxLength(500);

            builder.HasOne(t => t.Account)
                .WithMany(a => a.Transactions);

            builder.HasIndex(t => new { t.AccountId, t.DateTime })
                .HasDatabaseName("ix_transactions_account_id_date");

            builder.HasIndex(t => t.DateTime)
                .HasMethod("gist")
                .HasDatabaseName("ix_transactions_date_gist");
            // поле для оптимистичной блокировки
            builder.Property(t => t.Version)
                .IsRowVersion();
        }
    }
}
