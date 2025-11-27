using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddTestTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Tests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tests_TypeId",
                table: "Tests",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_TestTypes_TypeId",
                table: "Tests",
                column: "TypeId",
                principalTable: "TestTypes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tests_TestTypes_TypeId",
                table: "Tests");

            migrationBuilder.DropTable(
                name: "TestTypes");

            migrationBuilder.DropIndex(
                name: "IX_Tests_TypeId",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Tests");
        }
    }
}
