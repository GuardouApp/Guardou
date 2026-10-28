using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guardou.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddItemStars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Starred",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Items_OwnerId_Starred",
                table: "Items",
                columns: new[] { "OwnerId", "Starred" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_OwnerId_Starred",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Starred",
                table: "Items");
        }
    }
}
