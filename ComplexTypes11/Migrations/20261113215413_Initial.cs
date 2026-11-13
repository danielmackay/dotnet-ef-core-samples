using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplexTypes11.Migrations;

/// <inheritdoc />
public partial class _20261113215413_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence(
            name: "VehicleSequence");

        migrationBuilder.CreateTable(
            name: "Animals",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Details_BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                Details_Veterinarian = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Animals", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Cars",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [VehicleSequence]"),
                Spec_Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Spec_Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Doors = table.Column<int>(type: "int", nullable: false),
                Features = table.Column<string>(type: "json", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cars", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Number_Value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Address_PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Address_Street = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Contact = table.Column<string>(type: "json", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Number_Value);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Lines = table.Column<string>(type: "json", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Trucks",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [VehicleSequence]"),
                Spec_Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Spec_Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Axles = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Trucks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Cats",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false),
                IsIndoor = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cats", x => x.Id);
                table.ForeignKey(
                    name: "FK_Cats_Animals_Id",
                    column: x => x.Id,
                    principalTable: "Animals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Dogs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false),
                Breed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Extras = table.Column<string>(type: "json", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Dogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_Dogs_Animals_Id",
                    column: x => x.Id,
                    principalTable: "Animals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Address_PostalCode",
            table: "Customers",
            column: "Address_PostalCode");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Address_Street",
            table: "Customers",
            column: "Address_Street");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Contact_Address_City",
            table: "Customers",
            column: "Contact");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Cars");

        migrationBuilder.DropTable(
            name: "Cats");

        migrationBuilder.DropTable(
            name: "Customers");

        migrationBuilder.DropTable(
            name: "Dogs");

        migrationBuilder.DropTable(
            name: "Orders");

        migrationBuilder.DropTable(
            name: "Trucks");

        migrationBuilder.DropTable(
            name: "Animals");

        migrationBuilder.DropSequence(
            name: "VehicleSequence");
    }
}
