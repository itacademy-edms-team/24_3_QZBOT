using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class UserTestAnswer2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTestAnswer_UserTests_UserTestId",
                table: "UserTestAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTestAnswer",
                table: "UserTestAnswer");

            migrationBuilder.RenameTable(
                name: "UserTestAnswer",
                newName: "UserTestAnswers");

            migrationBuilder.RenameIndex(
                name: "IX_UserTestAnswer_UserTestId",
                table: "UserTestAnswers",
                newName: "IX_UserTestAnswers_UserTestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTestAnswers",
                table: "UserTestAnswers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTestAnswers_UserTests_UserTestId",
                table: "UserTestAnswers",
                column: "UserTestId",
                principalTable: "UserTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTestAnswers_UserTests_UserTestId",
                table: "UserTestAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTestAnswers",
                table: "UserTestAnswers");

            migrationBuilder.RenameTable(
                name: "UserTestAnswers",
                newName: "UserTestAnswer");

            migrationBuilder.RenameIndex(
                name: "IX_UserTestAnswers_UserTestId",
                table: "UserTestAnswer",
                newName: "IX_UserTestAnswer_UserTestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTestAnswer",
                table: "UserTestAnswer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTestAnswer_UserTests_UserTestId",
                table: "UserTestAnswer",
                column: "UserTestId",
                principalTable: "UserTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
