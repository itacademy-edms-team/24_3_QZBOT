using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class EditModelUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramId",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "User",
                newName: "FullName");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "User",
                newName: "Name");

            migrationBuilder.AddColumn<long>(
                name: "TelegramId",
                table: "User",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
