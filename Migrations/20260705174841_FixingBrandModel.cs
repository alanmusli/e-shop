using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_store.Migrations
{
    /// <inheritdoc />
    public partial class FixingBrandModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Brands",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Brands",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Brands");
        }
    }
}
