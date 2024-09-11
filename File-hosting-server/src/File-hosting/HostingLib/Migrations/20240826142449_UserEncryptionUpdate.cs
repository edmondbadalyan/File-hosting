using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostingLib.Migrations
{
    /// <inheritdoc />
    public partial class UserEncryptionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptionKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Iv",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "EncryptionKey",
                table: "Users",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Iv",
                table: "Users",
                type: "varbinary(16)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
