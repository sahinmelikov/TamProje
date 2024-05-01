using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QrSystem.Migrations
{
    /// <inheritdoc />
    public partial class activeall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActiveAll",
                table: "SaxlanilanS",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ActiveAll",
                table: "BasketİtemVM",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveAll",
                table: "SaxlanilanS");

            migrationBuilder.DropColumn(
                name: "ActiveAll",
                table: "BasketİtemVM");
        }
    }
}
