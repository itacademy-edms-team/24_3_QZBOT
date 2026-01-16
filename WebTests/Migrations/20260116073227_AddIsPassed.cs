using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class AddIsPassed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestTestTypes");

            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "UserTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "UserTests");

            migrationBuilder.CreateTable(
                name: "TestTestType",
                columns: table => new
                {
                    TestId = table.Column<int>(type: "int", nullable: false),
                    TestTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTestType", x => new { x.TestId, x.TestTypeId });
                    table.ForeignKey(
                        name: "FK_TestTestType_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestTestType_TestTypes_TestTypeId",
                        column: x => x.TestTypeId,
                        principalTable: "TestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestTestType_TestTypeId",
                table: "TestTestType",
                column: "TestTypeId");
        }
    }
}
