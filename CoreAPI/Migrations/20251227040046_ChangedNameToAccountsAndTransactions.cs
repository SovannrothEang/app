using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedNameToAccountsAndTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "LoyaltyAccounts");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    CustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Balance = table.Column<int>(type: "INT", nullable: false),
                    Tier = table.Column<byte>(type: "TINYINT", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => new { x.TenantId, x.CustomerId });
                    table.ForeignKey(
                        name: "FK_Accounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    CustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    Type = table.Column<byte>(type: "TINYINT", nullable: false),
                    Reason = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    ReferenceId = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "DATETIME2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_TenantId_CustomerId",
                        columns: x => new { x.TenantId, x.CustomerId },
                        principalTable: "Accounts",
                        principalColumns: new[] { "TenantId", "CustomerId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CustomerId",
                table: "Accounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId",
                table: "Accounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId_CustomerId",
                table: "Accounts",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Id",
                table: "Transactions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TenantId_CustomerId",
                table: "Transactions",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                table: "Transactions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.CreateTable(
                name: "LoyaltyAccounts",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    CustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Balance = table.Column<int>(type: "INT", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Tier = table.Column<byte>(type: "TINYINT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyAccounts", x => new { x.TenantId, x.CustomerId });
                    table.ForeignKey(
                        name: "FK_LoyaltyAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    CustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "DATETIME2(3)", nullable: false),
                    Reason = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    ReferenceId = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Type = table.Column<byte>(type: "TINYINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointTransactions_LoyaltyAccounts_TenantId_CustomerId",
                        columns: x => new { x.TenantId, x.CustomerId },
                        principalTable: "LoyaltyAccounts",
                        principalColumns: new[] { "TenantId", "CustomerId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyAccounts_CustomerId",
                table: "LoyaltyAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyAccounts_TenantId",
                table: "LoyaltyAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyAccounts_TenantId_CustomerId",
                table: "LoyaltyAccounts",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_Id",
                table: "PointTransactions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_TenantId_CustomerId",
                table: "PointTransactions",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_Type",
                table: "PointTransactions",
                column: "Type");
        }
    }
}
