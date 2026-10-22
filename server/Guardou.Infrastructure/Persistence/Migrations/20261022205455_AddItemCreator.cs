using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guardou.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddItemCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"Items\" SET \"CreatedBy\" = \"OwnerId\";");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedBy",
                table: "Items",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_AspNetUsers_CreatedBy",
                table: "Items",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_AspNetUsers_CreatedBy",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_CreatedBy",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Items");
        }
    }
}
