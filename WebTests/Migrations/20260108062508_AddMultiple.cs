using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddMultiple : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMultiple",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMultiple",
                table: "Questions");
        }
    }
}
