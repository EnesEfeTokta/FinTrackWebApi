using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinTrackWebApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyHistorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRates_CurrencySnapshots_CurrencySnapshotId ",
                table: "ExchangeRates");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_CurrencySnapshotId _TargetCurrency",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "TargetCurrency",
                table: "ExchangeRates");

            migrationBuilder.RenameColumn(
                name: "CurrencySnapshotId ",
                table: "ExchangeRates",
                newName: "CurrencySnapshotId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "ExchangeRates",
                type: "numeric(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,8)");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "ExchangeRates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "BaseCurrency",
                table: "CurrencySnapshots",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    CurrencyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    CountryName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvailableUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IconUrl = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    LastUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.CurrencyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencyId",
                table: "ExchangeRates",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencySnapshotId_CurrencyId",
                table: "ExchangeRates",
                columns: new[] { "CurrencySnapshotId", "CurrencyId" }
                /*unique: true*/);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRates_Currencies_CurrencyId",
                table: "ExchangeRates",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRates_CurrencySnapshots_CurrencySnapshotId",
                table: "ExchangeRates",
                column: "CurrencySnapshotId",
                principalTable: "CurrencySnapshots",
                principalColumn: "CurrencySnapshotId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRates_Currencies_CurrencyId",
                table: "ExchangeRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRates_CurrencySnapshots_CurrencySnapshotId",
                table: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_CurrencyId",
                table: "ExchangeRates");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_CurrencySnapshotId_CurrencyId",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "ExchangeRates");

            migrationBuilder.RenameColumn(
                name: "CurrencySnapshotId",
                table: "ExchangeRates",
                newName: "CurrencySnapshotId ");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "ExchangeRates",
                type: "numeric(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)");

            migrationBuilder.AddColumn<string>(
                name: "TargetCurrency",
                table: "ExchangeRates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "BaseCurrency",
                table: "CurrencySnapshots",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencySnapshotId _TargetCurrency",
                table: "ExchangeRates",
                columns: new[] { "CurrencySnapshotId ", "TargetCurrency" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRates_CurrencySnapshots_CurrencySnapshotId ",
                table: "ExchangeRates",
                column: "CurrencySnapshotId ",
                principalTable: "CurrencySnapshots",
                principalColumn: "CurrencySnapshotId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
