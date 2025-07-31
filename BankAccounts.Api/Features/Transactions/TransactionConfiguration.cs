using BankAccounts.Api.Features.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Features.Transactions;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(transaction => transaction.TransactionId);
        builder.HasIndex(transaction => transaction.TransactionId);
        builder.HasOne<Account>(transaction => transaction.Account)
            .WithMany()
            .HasForeignKey(transaction => transaction.AccountId);
    }
}