using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedGhostEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR(50)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2(3)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CAT_CHK_CODE_NOT_EMPTY", "[Code] <> ''");
                    table.CheckConstraint("CAT_CHK_NAME_NOT_EMPTY", "[Name] <> ''");
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR(50)", nullable: false),
                    Country = table.Column<string>(type: "NVARCHAR(50)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2(3)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    Symbol = table.Column<string>(type: "NVARCHAR(5)", nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                    table.CheckConstraint("CHK_CODE_NOT_EMPTY", "[Code] <> ''");
                    table.CheckConstraint("CHK_COUNTRY_NOT_EMPTY", "[Country] <> ''");
                    table.CheckConstraint("CHK_SYMBOL_NOT_EMPTY", "[Symbol] <> ''");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    CategoryId = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2(3)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "VARCHAR(5)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Price = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "INT", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CHK_CODE_NOT_EMPTY1", "[Code] <> ''");
                    table.CheckConstraint("CHK_NAME_NOT_EMPTY", "[Name] <> ''");
                    table.CheckConstraint("CHK_PRICE_POSITIVE", "[Price] >= 0.0");
                    table.CheckConstraint("CHK_STOCK_POSITIVE", "[Stock] >= 0");
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Code",
                table: "Categories",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Id",
                table: "Categories",
                column: "Id",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Id",
                table: "Currencies",
                column: "Id",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }
    }
}
