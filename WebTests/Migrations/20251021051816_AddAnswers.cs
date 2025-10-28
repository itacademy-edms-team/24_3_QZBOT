using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddAnswers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerOption_Questions_QuestionId",
                table: "AnswerOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnswerOption",
                table: "AnswerOption");

            migrationBuilder.RenameTable(
                name: "AnswerOption",
                newName: "AnswerOptions");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerOption_QuestionId",
                table: "AnswerOptions",
                newName: "IX_AnswerOptions_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnswerOptions",
                table: "AnswerOptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerOptions_Questions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerOptions_Questions_QuestionId",
                table: "AnswerOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnswerOptions",
                table: "AnswerOptions");

            migrationBuilder.RenameTable(
                name: "AnswerOptions",
                newName: "AnswerOption");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOption",
                newName: "IX_AnswerOption_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnswerOption",
                table: "AnswerOption",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerOption_Questions_QuestionId",
                table: "AnswerOption",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");
        }
    }
}
