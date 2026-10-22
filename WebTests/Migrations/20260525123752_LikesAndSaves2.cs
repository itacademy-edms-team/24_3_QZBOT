using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTests.Migrations
{
    public partial class LikesAndSaves2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LikedTest_AspNetUsers_UserId",
                table: "LikedTest");

            migrationBuilder.DropForeignKey(
                name: "FK_LikedTest_Tests_TestId",
                table: "LikedTest");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedTest_AspNetUsers_UserId",
                table: "SavedTest");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedTest_Tests_TestId",
                table: "SavedTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedTest",
                table: "SavedTest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LikedTest",
                table: "LikedTest");

            migrationBuilder.RenameTable(
                name: "SavedTest",
                newName: "SavedTests");

            migrationBuilder.RenameTable(
                name: "LikedTest",
                newName: "LikedTests");

            migrationBuilder.RenameIndex(
                name: "IX_SavedTest_TestId",
                table: "SavedTests",
                newName: "IX_SavedTests_TestId");

            migrationBuilder.RenameIndex(
                name: "IX_LikedTest_TestId",
                table: "LikedTests",
                newName: "IX_LikedTests_TestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedTests",
                table: "SavedTests",
                columns: new[] { "UserId", "TestId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_LikedTests",
                table: "LikedTests",
                columns: new[] { "UserId", "TestId" });

            migrationBuilder.AddForeignKey(
                name: "FK_LikedTests_AspNetUsers_UserId",
                table: "LikedTests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LikedTests_Tests_TestId",
                table: "LikedTests",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedTests_AspNetUsers_UserId",
                table: "SavedTests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedTests_Tests_TestId",
                table: "SavedTests",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LikedTests_AspNetUsers_UserId",
                table: "LikedTests");

            migrationBuilder.DropForeignKey(
                name: "FK_LikedTests_Tests_TestId",
                table: "LikedTests");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedTests_AspNetUsers_UserId",
                table: "SavedTests");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedTests_Tests_TestId",
                table: "SavedTests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedTests",
                table: "SavedTests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LikedTests",
                table: "LikedTests");

            migrationBuilder.RenameTable(
                name: "SavedTests",
                newName: "SavedTest");

            migrationBuilder.RenameTable(
                name: "LikedTests",
                newName: "LikedTest");

            migrationBuilder.RenameIndex(
                name: "IX_SavedTests_TestId",
                table: "SavedTest",
                newName: "IX_SavedTest_TestId");

            migrationBuilder.RenameIndex(
                name: "IX_LikedTests_TestId",
                table: "LikedTest",
                newName: "IX_LikedTest_TestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedTest",
                table: "SavedTest",
                columns: new[] { "UserId", "TestId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_LikedTest",
                table: "LikedTest",
                columns: new[] { "UserId", "TestId" });

            migrationBuilder.AddForeignKey(
                name: "FK_LikedTest_AspNetUsers_UserId",
                table: "LikedTest",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LikedTest_Tests_TestId",
                table: "LikedTest",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedTest_AspNetUsers_UserId",
                table: "SavedTest",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedTest_Tests_TestId",
                table: "SavedTest",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
