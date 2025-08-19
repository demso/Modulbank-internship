using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Identity.Identity
{
    /// <summary>
    /// Конфигурация сущности пользователя
    /// </summary>
    public class BankUserConfiguration : IEntityTypeConfiguration<BankUser>
    {
        /// <summary>
        /// Конфигурирует пользовательскую сущность. Пользователь имеет первичный ключ по Id
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<BankUser> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}