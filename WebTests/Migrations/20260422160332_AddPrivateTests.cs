using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddPrivateTests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Tests");
        }
    }
}
