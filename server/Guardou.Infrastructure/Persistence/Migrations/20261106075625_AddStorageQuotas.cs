using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guardou.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "QuotaBytes",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UsedBytes",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(
                "UPDATE \"AspNetUsers\" SET \"UsedBytes\" = (SELECT COALESCE(SUM(\"Size\"), 0) FROM \"Items\" WHERE \"Items\".\"OwnerId\" = \"AspNetUsers\".\"Id\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuotaBytes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UsedBytes",
                table: "AspNetUsers");
        }
    }
}
