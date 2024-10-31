using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderData",
                table: "OrderHeaders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "OederTotal",
                table: "OrderHeaders",
                newName: "OrderTotal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderTotal",
                table: "OrderHeaders",
                newName: "OederTotal");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "OrderHeaders",
                newName: "OrderData");
        }
    }
}
