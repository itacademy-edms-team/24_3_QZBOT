using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddTestCreator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Tests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tests_CreatorId",
                table: "Tests",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_AspNetUsers_CreatorId",
                table: "Tests",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tests_AspNetUsers_CreatorId",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_Tests_CreatorId",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Tests");
        }
    }
}
