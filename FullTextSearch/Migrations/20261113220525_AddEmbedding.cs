using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FullTextSearch.Migrations;

/// <inheritdoc />
public partial class _20261113220525_AddEmbedding : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<SqlVector<float>>(
            name: "Embedding",
            table: "Articles",
            type: "vector(4)",
            nullable: false);
        // Edited by hand: EF scaffolded defaultValue: new SqlVector<float>(new[] { 0f, 0f, 0f, 0f }),
        // but SQL Server can't put a DEFAULT constraint on a vector column (error 1752). This sample's
        // table is empty when the migration runs. With existing rows, add the column as nullable,
        // fill in the embeddings, then make it NOT NULL.
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Embedding",
            table: "Articles");
    }
}
