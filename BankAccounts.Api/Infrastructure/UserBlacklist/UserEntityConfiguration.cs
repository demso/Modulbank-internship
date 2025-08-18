using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.UserBlacklist
{
    /// <summary>
    /// Конфигурация сущности пользователя
    /// </summary>
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.UserId).IsUnique();
        }
    }
}
