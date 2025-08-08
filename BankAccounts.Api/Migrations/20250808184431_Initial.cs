using BankAccounts.Api.Features.Accounts;
using BankAccounts.Api.Features.Transactions;
using BankAccounts.Api.Infrastructure.CurrencyService;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BankAccounts.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type", "checking,credit,deposit")
                .Annotation("Npgsql:Enum:currencies", "eur,rub,usd")
                .Annotation("Npgsql:Enum:transaction_type", "credit,debit");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountType = table.Column<AccountType>(type: "account_type", nullable: false),
                    Currency = table.Column<Currencies>(type: "currencies", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    InterestRate = table.Column<decimal>(type: "numeric", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    CounterpartyAccountId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<Currencies>(type: "currencies", nullable: false),
                    TransactionType = table.Column<TransactionType>(type: "transaction_type", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OwnerId",
                table: "Accounts",
                column: "OwnerId")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_account_id_date",
                table: "Transactions",
                columns: new[] { "AccountId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_date_gist",
                table: "Transactions",
                column: "DateTime")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
