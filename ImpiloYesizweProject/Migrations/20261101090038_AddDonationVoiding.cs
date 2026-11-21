using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImpiloYesizweProject.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationVoiding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "Donations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "Donations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAtUtc",
                table: "Donations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidedBy",
                table: "Donations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "VoidedAtUtc",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "VoidedBy",
                table: "Donations");
        }
    }
}
