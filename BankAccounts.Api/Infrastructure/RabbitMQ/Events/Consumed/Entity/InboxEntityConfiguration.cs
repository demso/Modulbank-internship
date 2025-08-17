using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity
{
    /// <inheritdoc />
    public class InboxEntityConfiguration :  IEntityTypeConfiguration<InboxConsumedEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<InboxConsumedEntity> builder)
        {
            builder.ToTable("inbox_consumed");
            builder.HasKey(i => i.Id);
            builder.HasIndex(i => i.MessageId)
                .IsUnique();
            builder.Property(i => i.Handler)
                .HasMaxLength(256);
            
        }
    }
}
