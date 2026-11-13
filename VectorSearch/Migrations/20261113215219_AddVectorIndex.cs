using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VectorSearch.Migrations;

/// <inheritdoc />
public partial class _20261113215219_AddVectorIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Hand-added: vector indexes are a preview feature in SQL Server 2025, so the database
        // must opt in before CREATE VECTOR INDEX. ALTER DATABASE SCOPED CONFIGURATION can't run
        // inside a transaction, hence suppressTransaction.
        migrationBuilder.Sql(
            "ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;",
            suppressTransaction: true);

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Embedding",
            table: "Articles",
            column: "Embedding")
            .Annotation("SqlServer:VectorIndexMetric", "cosine");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Articles_Embedding",
            table: "Articles");
    }
}
