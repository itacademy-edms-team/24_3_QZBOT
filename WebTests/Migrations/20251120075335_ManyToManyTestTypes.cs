using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class ManyToManyTestTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tests_TestTypes_TypeId",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_Tests_TypeId",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Tests");

            migrationBuilder.CreateTable(
                name: "TestTestTypes",
                columns: table => new
                {
                    TestsId = table.Column<int>(type: "int", nullable: false),
                    TypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTestTypes", x => new { x.TestsId, x.TypesId });
                    table.ForeignKey(
                        name: "FK_TestTestTypes_Tests_TestsId",
                        column: x => x.TestsId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestTestTypes_TestTypes_TypesId",
                        column: x => x.TypesId,
                        principalTable: "TestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestTestTypes_TypesId",
                table: "TestTestTypes",
                column: "TypesId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestTestTypes");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Tests",
                type: "int",
                nullable: true);

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
    }
}
