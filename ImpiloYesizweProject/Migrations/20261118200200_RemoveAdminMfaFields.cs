using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImpiloYesizweProject.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdminMfaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMfaTimeStep",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "MfaEnabled",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "MfaSecretProtected",
                table: "AdminUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastMfaTimeStep",
                table: "AdminUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MfaEnabled",
                table: "AdminUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MfaSecretProtected",
                table: "AdminUsers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
