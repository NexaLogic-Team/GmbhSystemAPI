using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GmbhSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroMediaUrl",
                table: "HomeSections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "HomeSections",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroMediaUrl",
                table: "HomeSections");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "HomeSections");
        }
    }
}
