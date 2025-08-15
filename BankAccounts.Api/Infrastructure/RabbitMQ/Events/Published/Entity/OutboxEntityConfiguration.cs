using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Published.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events
{
    public class OutboxEntityConfiguration : IEntityTypeConfiguration<OutboxPublishedEntity>
    {
        public void Configure(EntityTypeBuilder<OutboxPublishedEntity> builder)
        {
            builder.ToTable("outbox_published");
            builder.HasKey(o => o.Id);
        }
    }
}
