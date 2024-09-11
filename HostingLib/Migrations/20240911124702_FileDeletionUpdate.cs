using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostingLib.Migrations
{
    /// <inheritdoc />
    public partial class FileDeletionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AutoFileDeletionTime",
                table: "Users",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(30, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoFileDeletionTime",
                table: "Users");
        }
    }
}
