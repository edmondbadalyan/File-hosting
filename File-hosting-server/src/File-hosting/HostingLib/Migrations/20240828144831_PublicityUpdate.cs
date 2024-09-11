using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostingLib.Migrations
{
    /// <inheritdoc />
    public partial class PublicityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Files",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Files");
        }
    }
}
