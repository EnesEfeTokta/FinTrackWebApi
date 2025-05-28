using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinTrackWebApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateVideoMetadataAndDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoMetadatas",
                columns: table => new
                {
                    VideoMetadataId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DebtId = table.Column<int>(type: "integer", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: true),
                    StoredFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    UploadDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StorageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoMetadatas", x => x.VideoMetadataId);
                    table.ForeignKey(
                        name: "FK_VideoMetadatas_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Debts",
                columns: table => new
                {
                    DebtId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LenderId = table.Column<int>(type: "integer", nullable: false),
                    BorrowerId = table.Column<int>(type: "integer", nullable: false),
                    VideoMetadataId = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreateAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Debts", x => x.DebtId);
                    table.ForeignKey(
                        name: "FK_Debts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Debts_Users_BorrowerId",
                        column: x => x.BorrowerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Debts_Users_LenderId",
                        column: x => x.LenderId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Debts_VideoMetadatas_VideoMetadataId",
                        column: x => x.VideoMetadataId,
                        principalTable: "VideoMetadatas",
                        principalColumn: "VideoMetadataId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Debts_BorrowerId",
                table: "Debts",
                column: "BorrowerId");

            migrationBuilder.CreateIndex(
                name: "IX_Debts_CurrencyId",
                table: "Debts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Debts_LenderId",
                table: "Debts",
                column: "LenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Debts_VideoMetadataId",
                table: "Debts",
                column: "VideoMetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoMetadatas_UploadedByUserId",
                table: "VideoMetadatas",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Debts");

            migrationBuilder.DropTable(
                name: "VideoMetadatas");
        }
    }
}
