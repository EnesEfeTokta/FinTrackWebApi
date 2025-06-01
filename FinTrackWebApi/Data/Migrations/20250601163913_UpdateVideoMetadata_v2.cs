using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrackWebApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVideoMetadata_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "VideoMetadatas");

            migrationBuilder.AlterColumn<string>(
                name: "StoredFileName",
                table: "VideoMetadatas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "StorageType",
                table: "VideoMetadatas",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedFilePath",
                table: "VideoMetadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionIV",
                table: "VideoMetadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKeyHash",
                table: "VideoMetadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptionSalt",
                table: "VideoMetadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "VideoMetadatas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnencryptedFilePath",
                table: "VideoMetadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Debts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedFilePath",
                table: "VideoMetadatas");

            migrationBuilder.DropColumn(
                name: "EncryptionIV",
                table: "VideoMetadatas");

            migrationBuilder.DropColumn(
                name: "EncryptionKeyHash",
                table: "VideoMetadatas");

            migrationBuilder.DropColumn(
                name: "EncryptionSalt",
                table: "VideoMetadatas");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VideoMetadatas");

            migrationBuilder.DropColumn(
                name: "UnencryptedFilePath",
                table: "VideoMetadatas");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Debts");

            migrationBuilder.AlterColumn<string>(
                name: "StoredFileName",
                table: "VideoMetadatas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "StorageType",
                table: "VideoMetadatas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "VideoMetadatas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
