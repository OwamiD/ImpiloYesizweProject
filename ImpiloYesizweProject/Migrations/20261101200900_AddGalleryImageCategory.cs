using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImpiloYesizweProject.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryImageCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "GalleryImages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "GalleryImages");
        }
    }
}
