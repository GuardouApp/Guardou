using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guardou.Infrastructure.Indexing.Migrations
{
    /// <inheritdoc />
    public partial class CreateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndexEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexEntries_ItemId",
                table: "IndexEntries",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndexEntries_OwnerId",
                table: "IndexEntries",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_IndexEntries_Path",
                table: "IndexEntries",
                column: "Path");

            migrationBuilder.Sql("""
                CREATE VIRTUAL TABLE "IndexNames" USING fts5("Name", content='IndexEntries', content_rowid='Id', tokenize='trigram');
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER "IndexEntries_Insert" AFTER INSERT ON "IndexEntries" BEGIN
                    INSERT INTO "IndexNames"(rowid, "Name") VALUES (new."Id", new."Name");
                END;
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER "IndexEntries_Delete" AFTER DELETE ON "IndexEntries" BEGIN
                    INSERT INTO "IndexNames"("IndexNames", rowid, "Name") VALUES ('delete', old."Id", old."Name");
                END;
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER "IndexEntries_Rename" AFTER UPDATE OF "Name" ON "IndexEntries" BEGIN
                    INSERT INTO "IndexNames"("IndexNames", rowid, "Name") VALUES ('delete', old."Id", old."Name");
                    INSERT INTO "IndexNames"(rowid, "Name") VALUES (new."Id", new."Name");
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TABLE "IndexNames";""");
            migrationBuilder.DropTable(
                name: "IndexEntries");
        }
    }
}
