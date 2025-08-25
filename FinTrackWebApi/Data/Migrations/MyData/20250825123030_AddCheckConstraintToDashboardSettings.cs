using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinTrackWebApi.Data.Migrations.MyData
{
    /// <inheritdoc />
    public partial class AddCheckConstraintToDashboardSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "UserAppSettings",
                type: "text",
                nullable: false,
                defaultValue: "tr_TR",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Turkish");

            migrationBuilder.CreateTable(
                name: "UserDashboardSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SelectedCurrencies = table.Column<string>(type: "text", nullable: false),
                    SelectedBudgets = table.Column<string>(type: "text", nullable: false),
                    SelectedAccounts = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDashboardSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDashboardSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDashboardSettings_UserId",
                table: "UserDashboardSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDashboardSettings");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "UserAppSettings",
                type: "text",
                nullable: false,
                defaultValue: "Turkish",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "tr_TR");
        }
    }
}
