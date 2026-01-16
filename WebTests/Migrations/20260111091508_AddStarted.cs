using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddStarted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassedAt",
                table: "UserTests",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "IsPassed",
                table: "UserTests",
                newName: "IsFinished");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "UserTests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinSuccessPercent",
                table: "Tests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "UserTests");

            migrationBuilder.DropColumn(
                name: "MinSuccessPercent",
                table: "Tests");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "UserTests",
                newName: "PassedAt");

            migrationBuilder.RenameColumn(
                name: "IsFinished",
                table: "UserTests",
                newName: "IsPassed");
        }
    }
}
