using System;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankAccounts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type", "checking,credit,deposit")
                .Annotation("Npgsql:Enum:currencies", "eur,rub,usd")
                .Annotation("Npgsql:Enum:event_type", "account_opened,client_blocked,client_unblocked,interest_accrued,money_credited,money_debited,transfer_completed")
                .Annotation("Npgsql:Enum:transaction_type", "credit,debit")
                .OldAnnotation("Npgsql:Enum:account_type", "checking,credit,deposit")
                .OldAnnotation("Npgsql:Enum:currencies", "eur,rub,usd")
                .OldAnnotation("Npgsql:Enum:transaction_type", "credit,debit");

            migrationBuilder.CreateTable(
                name: "inbox_consumed",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<EventType>(type: "event_type", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Handler = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_consumed", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "inbox_dead_letters",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Handler = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<EventType>(type: "event_type", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_dead_letters", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "outbox_published",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<EventType>(type: "event_type", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TryCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_published", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_consumed");

            migrationBuilder.DropTable(
                name: "inbox_dead_letters");

            migrationBuilder.DropTable(
                name: "outbox_published");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type", "checking,credit,deposit")
                .Annotation("Npgsql:Enum:currencies", "eur,rub,usd")
                .Annotation("Npgsql:Enum:transaction_type", "credit,debit")
                .OldAnnotation("Npgsql:Enum:account_type", "checking,credit,deposit")
                .OldAnnotation("Npgsql:Enum:currencies", "eur,rub,usd")
                .OldAnnotation("Npgsql:Enum:event_type", "account_opened,client_blocked,client_unblocked,interest_accrued,money_credited,money_debited,transfer_completed")
                .OldAnnotation("Npgsql:Enum:transaction_type", "credit,debit");
        }
    }
}
