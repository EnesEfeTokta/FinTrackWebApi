using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrackWebApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "OtpVerifications");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "OtpVerifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "OtpVerifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "OtpVerifications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "OtpVerifications");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "OtpVerifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
