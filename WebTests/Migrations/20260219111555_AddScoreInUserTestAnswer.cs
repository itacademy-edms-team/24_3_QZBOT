using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddScoreInUserTestAnswer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "UserTestAnswers");

            migrationBuilder.AlterColumn<double>(
                name: "Score",
                table: "UserTests",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "UserTestAnswers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "UserTestAnswers");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "UserTests",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "UserTestAnswers",
                type: "bit",
                nullable: true);
        }
    }
}
