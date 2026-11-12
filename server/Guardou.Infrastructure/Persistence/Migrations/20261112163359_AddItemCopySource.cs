using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guardou.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddItemCopySource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CopySourceKey",
                table: "Items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CopySourceKey",
                table: "Items",
                column: "CopySourceKey",
                filter: "\"CopySourceKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_CopySourceKey",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CopySourceKey",
                table: "Items");
        }
    }
}
