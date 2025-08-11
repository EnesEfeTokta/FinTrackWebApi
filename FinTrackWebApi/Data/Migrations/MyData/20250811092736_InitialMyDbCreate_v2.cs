using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrackWebApi.Data.Migrations.MyData
{
    /// <inheritdoc />
    public partial class InitialMyDbCreate_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "UserAppSettings",
                type: "text",
                nullable: false,
                defaultValue: "Turkish");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "UserAppSettings");
        }
    }
}
