using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FullTextSearch.Migrations;

/// <inheritdoc />
public partial class _20261113215918_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Added by hand. HasFullTextCatalog breaks 'migrations add' in EF Core 11.0.0 (dotnet/efcore#39599),
        // so the catalog isn't in the model. Full-text DDL can't run inside a transaction.
        migrationBuilder.Sql("CREATE FULLTEXT CATALOG [ftCatalog];", suppressTransaction: true);

        migrationBuilder.CreateTable(
            name: "Articles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Contents = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Articles", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Title_Contents",
            table: "Articles",
            columns: new[] { "Title", "Contents" })
            .Annotation("SqlServer:FullTextCatalog", "ftCatalog")
            .Annotation("SqlServer:FullTextIndex", "PK_Articles");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Articles");

        // Added by hand, see Up().
        migrationBuilder.Sql("DROP FULLTEXT CATALOG [ftCatalog];", suppressTransaction: true);
    }
}
