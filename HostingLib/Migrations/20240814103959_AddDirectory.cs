using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostingLib.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDirectory",
                table: "Files",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Files",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Files_ParentId",
                table: "Files",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Files_ParentId",
                table: "Files",
                column: "ParentId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Files_ParentId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ParentId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IsDirectory",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Files");
        }
    }
}
