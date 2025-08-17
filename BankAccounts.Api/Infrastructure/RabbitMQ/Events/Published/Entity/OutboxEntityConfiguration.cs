using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity
{
    /// <inheritdoc />
    public class OutboxEntityConfiguration : IEntityTypeConfiguration<OutboxPublishedEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<OutboxPublishedEntity> builder)
        {
            builder.ToTable("outbox_published")
                .HasKey(o => o.Id);
            builder.Property(o => o.Message)
                .HasMaxLength(int.MaxValue);
        }
    }
}
