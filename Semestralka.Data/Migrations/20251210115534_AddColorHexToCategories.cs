using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Semestralka.Migrations
{
    /// <inheritdoc />
    public partial class AddColorHexToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Categories",
                newName: "ColorHex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ColorHex",
                table: "Categories",
                newName: "Color");
        }
    }
}
