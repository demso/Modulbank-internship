using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter
{
    public class DeadLetterEntityConfiguration : IEntityTypeConfiguration<DeadLetterEntity>
    {
        public void Configure(EntityTypeBuilder<DeadLetterEntity> builder)
        {
            builder.ToTable("inbox_dead_letters");
            builder.HasKey(l => l.MessageId);
        }
    }
}
