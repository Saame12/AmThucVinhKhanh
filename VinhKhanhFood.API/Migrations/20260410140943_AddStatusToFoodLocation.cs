using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhKhanhFood.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToFoodLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FoodLocations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "FoodLocations");
        }
    }
}
