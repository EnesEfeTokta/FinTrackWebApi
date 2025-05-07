using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinTrackWebApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCurrencyHistorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencySnapshots",
                columns: table => new
                {
                    CurrencySnapshotId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FetchTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BaseCurrency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencySnapshots", x => x.CurrencySnapshotId);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    ExchangeRateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetCurrency = table.Column<string>(type: "text", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    CurrencySnapshotId = table.Column<int>(name: "CurrencySnapshotId ", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.ExchangeRateId);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_CurrencySnapshots_CurrencySnapshotId ",
                        column: x => x.CurrencySnapshotId,
                        principalTable: "CurrencySnapshots",
                        principalColumn: "CurrencySnapshotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencySnapshots_FetchTimestamp",
                table: "CurrencySnapshots",
                column: "FetchTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencySnapshotId _TargetCurrency",
                table: "ExchangeRates",
                columns: new[] { "CurrencySnapshotId ", "TargetCurrency" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "CurrencySnapshots");
        }
    }
}
