using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkToUserTechnology : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UsersTechnologies_TechnologyId",
                table: "UsersTechnologies",
                column: "TechnologyId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersTechnologies_Technologies_TechnologyId",
                table: "UsersTechnologies",
                column: "TechnologyId",
                principalTable: "Technologies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersTechnologies_Technologies_TechnologyId",
                table: "UsersTechnologies");

            migrationBuilder.DropIndex(
                name: "IX_UsersTechnologies_TechnologyId",
                table: "UsersTechnologies");
        }
    }
}
