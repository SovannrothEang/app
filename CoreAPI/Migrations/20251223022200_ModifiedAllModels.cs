using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedAllModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_LoyaltyAccounts_CustomerId_TenantId",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_CustomerId_TenantId",
                table: "PointTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoyaltyAccounts",
                table: "LoyaltyAccounts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "PointTransactions",
                type: "INT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");

            migrationBuilder.AlterColumn<byte>(
                name: "Tier",
                table: "LoyaltyAccounts",
                type: "TINYINT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "LoyaltyAccounts",
                type: "INT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LoyaltyAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LoyaltyAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LoyaltyAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyAccounts",
                table: "LoyaltyAccounts",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_TenantId_CustomerId",
                table: "PointTransactions",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyAccounts_CustomerId",
                table: "LoyaltyAccounts",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_LoyaltyAccounts_TenantId_CustomerId",
                table: "PointTransactions",
                columns: new[] { "TenantId", "CustomerId" },
                principalTable: "LoyaltyAccounts",
                principalColumns: new[] { "TenantId", "CustomerId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_LoyaltyAccounts_TenantId_CustomerId",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_TenantId_CustomerId",
                table: "PointTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoyaltyAccounts",
                table: "LoyaltyAccounts");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyAccounts_CustomerId",
                table: "LoyaltyAccounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LoyaltyAccounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LoyaltyAccounts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LoyaltyAccounts");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PointTransactions",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INT");

            migrationBuilder.AlterColumn<int>(
                name: "Tier",
                table: "LoyaltyAccounts",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "TINYINT");

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "LoyaltyAccounts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoyaltyAccounts",
                table: "LoyaltyAccounts",
                columns: new[] { "CustomerId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CustomerId_TenantId",
                table: "PointTransactions",
                columns: new[] { "CustomerId", "TenantId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_LoyaltyAccounts_CustomerId_TenantId",
                table: "PointTransactions",
                columns: new[] { "CustomerId", "TenantId" },
                principalTable: "LoyaltyAccounts",
                principalColumns: new[] { "CustomerId", "TenantId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
