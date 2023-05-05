using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MigrationBundles.Migrations
{
    /// <inheritdoc />
    public partial class Prodcut_Timestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTs",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTs",
                table: "Products",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedTs",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedTs",
                table: "Products");
        }
    }
}
