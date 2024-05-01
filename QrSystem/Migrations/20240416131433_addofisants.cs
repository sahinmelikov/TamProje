using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QrSystem.Migrations
{
    /// <inheritdoc />
    public partial class addofisants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timer",
                table: "SaxlanilanS");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "BasketİtemVM");

            migrationBuilder.RenameColumn(
                name: "SifarisSayi",
                table: "Tables",
                newName: "OfisantId");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "SaxlanilanS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfisantName",
                table: "SaxlanilanS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BigParentCategoryId",
                table: "ParentsCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RestorantId",
                table: "ParentsCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "BasketİtemVM",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "BasketİtemVM",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeExpired",
                table: "BasketİtemVM",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OfisantName",
                table: "BasketİtemVM",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BigParentCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RestorantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BigParentCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BigParentCategory_Restorant_RestorantId",
                        column: x => x.RestorantId,
                        principalTable: "Restorant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ofisant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RestorantId = table.Column<int>(type: "int", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ofisant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ofisant_Restorant_RestorantId",
                        column: x => x.RestorantId,
                        principalTable: "Restorant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tables_OfisantId",
                table: "Tables",
                column: "OfisantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentsCategories_BigParentCategoryId",
                table: "ParentsCategories",
                column: "BigParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentsCategories_RestorantId",
                table: "ParentsCategories",
                column: "RestorantId");

            migrationBuilder.CreateIndex(
                name: "IX_BigParentCategory_RestorantId",
                table: "BigParentCategory",
                column: "RestorantId");

            migrationBuilder.CreateIndex(
                name: "IX_Ofisant_RestorantId",
                table: "Ofisant",
                column: "RestorantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentsCategories_BigParentCategory_BigParentCategoryId",
                table: "ParentsCategories",
                column: "BigParentCategoryId",
                principalTable: "BigParentCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentsCategories_Restorant_RestorantId",
                table: "ParentsCategories",
                column: "RestorantId",
                principalTable: "Restorant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Ofisant_OfisantId",
                table: "Tables",
                column: "OfisantId",
                principalTable: "Ofisant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentsCategories_BigParentCategory_BigParentCategoryId",
                table: "ParentsCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentsCategories_Restorant_RestorantId",
                table: "ParentsCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Ofisant_OfisantId",
                table: "Tables");

            migrationBuilder.DropTable(
                name: "BigParentCategory");

            migrationBuilder.DropTable(
                name: "Ofisant");

            migrationBuilder.DropIndex(
                name: "IX_Tables_OfisantId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_ParentsCategories_BigParentCategoryId",
                table: "ParentsCategories");

            migrationBuilder.DropIndex(
                name: "IX_ParentsCategories_RestorantId",
                table: "ParentsCategories");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "SaxlanilanS");

            migrationBuilder.DropColumn(
                name: "OfisantName",
                table: "SaxlanilanS");

            migrationBuilder.DropColumn(
                name: "BigParentCategoryId",
                table: "ParentsCategories");

            migrationBuilder.DropColumn(
                name: "RestorantId",
                table: "ParentsCategories");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "BasketİtemVM");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "BasketİtemVM");

            migrationBuilder.DropColumn(
                name: "IsTimeExpired",
                table: "BasketİtemVM");

            migrationBuilder.DropColumn(
                name: "OfisantName",
                table: "BasketİtemVM");

            migrationBuilder.RenameColumn(
                name: "OfisantId",
                table: "Tables",
                newName: "SifarisSayi");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timer",
                table: "SaxlanilanS",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "BasketİtemVM",
                type: "datetime2",
                nullable: true);
        }
    }
}
