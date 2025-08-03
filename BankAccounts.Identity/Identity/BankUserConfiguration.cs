using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Identity;

public class BankUserConfiguration : IEntityTypeConfiguration<BankUser>
{
    public void Configure(EntityTypeBuilder<BankUser> builder)
    {
        builder.HasKey(x => x.Id);
    }
}