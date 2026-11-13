using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations11.Catalog.Migrations;

/// <inheritdoc />
public partial class _20261113215331_AddProductRating : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Rating",
            table: "Products",
            type: "int",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Rating",
            table: "Products");
    }
}
