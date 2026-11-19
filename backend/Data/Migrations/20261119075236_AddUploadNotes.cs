using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ArchiveUploadId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Documents",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArchiveUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveUploads", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ArchiveUploadId",
                table: "Documents",
                column: "ArchiveUploadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ArchiveUploads_ArchiveUploadId",
                table: "Documents",
                column: "ArchiveUploadId",
                principalTable: "ArchiveUploads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ArchiveUploads_ArchiveUploadId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "ArchiveUploads");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ArchiveUploadId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ArchiveUploadId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Documents");
        }
    }
}
