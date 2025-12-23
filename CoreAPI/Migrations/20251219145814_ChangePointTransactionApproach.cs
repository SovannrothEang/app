using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePointTransactionApproach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointTransaction");

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    CustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    Type = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    ReferenceId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "DATETIME2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointTransactions_LoyaltyAccounts_CustomerId_TenantId",
                        columns: x => new { x.CustomerId, x.TenantId },
                        principalTable: "LoyaltyAccounts",
                        principalColumns: new[] { "CustomerId", "TenantId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CustomerId_TenantId",
                table: "PointTransactions",
                columns: new[] { "CustomerId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_Id",
                table: "PointTransactions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_Type",
                table: "PointTransactions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.CreateTable(
                name: "PointTransaction",
                columns: table => new
                {
                    LoyaltyAccountCustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    LoyaltyAccountTenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Point = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransaction", x => new { x.LoyaltyAccountCustomerId, x.LoyaltyAccountTenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_PointTransaction_LoyaltyAccounts_LoyaltyAccountCustomerId_LoyaltyAccountTenantId",
                        columns: x => new { x.LoyaltyAccountCustomerId, x.LoyaltyAccountTenantId },
                        principalTable: "LoyaltyAccounts",
                        principalColumns: new[] { "CustomerId", "TenantId" },
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
