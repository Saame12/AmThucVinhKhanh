using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhKhanhFood.API.Migrations
{
    /// <inheritdoc />
    public partial class FixMultiLangAndAudio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioUrl_EN",
                table: "FoodLocations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl_ZH",
                table: "FoodLocations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description_ZH",
                table: "FoodLocations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_ZH",
                table: "FoodLocations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrl_EN",
                table: "FoodLocations");

            migrationBuilder.DropColumn(
                name: "AudioUrl_ZH",
                table: "FoodLocations");

            migrationBuilder.DropColumn(
                name: "Description_ZH",
                table: "FoodLocations");

            migrationBuilder.DropColumn(
                name: "Name_ZH",
                table: "FoodLocations");
        }
    }
}
