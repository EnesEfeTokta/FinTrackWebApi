using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrackWebApi.Data.Migrations.MyData
{
    /// <inheritdoc />
    public partial class AddCheckConstraintToDashboardSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "UserDashboardSettings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "UserDashboardSettings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "UserDashboardSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "UserDashboardSettings");
        }
    }
}
