using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity
{
    public class InboxEntityConfiguration :  IEntityTypeConfiguration<InboxConsumedEntity>
    {
        public void Configure(EntityTypeBuilder<InboxConsumedEntity> builder)
        {
            builder.ToTable("inbox_consumed");
            builder.HasKey(i => i.MessageId);
        }
    }
}
