using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QrSystem.Migrations
{
    /// <inheritdoc />
    public partial class addofisants1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentsCategories_BigParentCategory_BigParentCategoryId",
                table: "ParentsCategories");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.RenameColumn(
                name: "BigParentCategoryId",
                table: "ParentsCategories",
                newName: "bigParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ParentsCategories_BigParentCategoryId",
                table: "ParentsCategories",
                newName: "IX_ParentsCategories_bigParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentsCategories_BigParentCategory_bigParentCategoryId",
                table: "ParentsCategories",
                column: "bigParentCategoryId",
                principalTable: "BigParentCategory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentsCategories_BigParentCategory_bigParentCategoryId",
                table: "ParentsCategories");

            migrationBuilder.RenameColumn(
                name: "bigParentCategoryId",
                table: "ParentsCategories",
                newName: "BigParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ParentsCategories_bigParentCategoryId",
                table: "ParentsCategories",
                newName: "IX_ParentsCategories_BigParentCategoryId");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ParentsCategories_BigParentCategory_BigParentCategoryId",
                table: "ParentsCategories",
                column: "BigParentCategoryId",
                principalTable: "BigParentCategory",
                principalColumn: "Id");
        }
    }
}
