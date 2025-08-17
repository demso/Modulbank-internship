using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter
{
    /// <summary>
    /// Конфигурация сущности таблицы с мертвыми сообщениями inbox_dead_letter <see cref="DeadLetterEntity"/>
    /// </summary>
    public class DeadLetterEntityConfiguration : IEntityTypeConfiguration<DeadLetterEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<DeadLetterEntity> builder)
        {
            builder.ToTable("inbox_dead_letters");
            builder.HasKey(l => l.Id);
            builder.HasIndex(l => l.MessageId)
                .IsUnique();
            builder.Property(l => l.Handler)
                .HasMaxLength(256);
            builder.Property(l => l.Error)
                .HasMaxLength(256);
            builder.Property(l => l.Payload)
                .HasMaxLength(int.MaxValue);
        }
    }
}
