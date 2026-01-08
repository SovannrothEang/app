using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the new table first
            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Slug = table.Column<string>(type: "VARCHAR(15)", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(15)", nullable: false),
                    Description = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    Url = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Multiplier = table.Column<int>(type: "INT", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionTypes_Users_PerformBy",
                        column: x => x.PerformBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            // 2. Add the foreign key column as Nullable initially
            migrationBuilder.AddColumn<string>(
                name: "TransactionTypeId",
                table: "Transactions",
                type: "VARCHAR(100)",
                nullable: true);

            // 3. Seed Data
            migrationBuilder.Sql(@"
                INSERT INTO TransactionTypes (Id, Slug, Name, Description, Url, TenantId, Multiplier, IsActive, IsDeleted, CreatedAt)
                SELECT CAST(NEWID() AS VARCHAR(100)), 'earn', 'Earn', 'Points earned from activities', '/api/tenants/{tenantId}/customers/{customerId}/redeem', Id, 1, 1, 0, SYSDATETIMEOFFSET() FROM Tenants;
                
                INSERT INTO TransactionTypes (Id, Slug, Name, Description, Url, TenantId, Multiplier, IsActive, IsDeleted, CreatedAt)
                SELECT CAST(NEWID() AS VARCHAR(100)), 'redeem', 'Redeem', 'Points redeemed for rewards', '/api/tenants/{tenantId}/customers/{customerId}/redeem', Id, -1, 1, 0, SYSDATETIMEOFFSET() FROM Tenants;
                
                INSERT INTO TransactionTypes (Id, Slug, Name, Description, Url, TenantId, Multiplier, IsActive, IsDeleted, CreatedAt)
                SELECT CAST(NEWID() AS VARCHAR(100)), 'adjust', 'Adjustment', 'Manual points adjustment', '/api/tenants/{tenantId}/customers/{customerId}/adjust', Id, 1, 1, 0, SYSDATETIMEOFFSET() FROM Tenants;
            ");

            // 4. Update Transactions
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.TransactionTypeId = tt.Id
                FROM Transactions t
                JOIN TransactionTypes tt ON t.TenantId = tt.TenantId
                WHERE (t.Type = 1 AND tt.Slug = 'earn')
                   OR (t.Type = 2 AND tt.Slug = 'redeem')
                   OR (t.Type = 3 AND tt.Slug = 'adjust');
            ");

            // 5. Make TransactionTypeId Required
            migrationBuilder.AlterColumn<string>(
                name: "TransactionTypeId",
                table: "Transactions",
                type: "VARCHAR(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)",
                oldNullable: true);

            // 6. Add Indexes and Constraints
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionTypeId",
                table: "Transactions",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_Id",
                table: "TransactionTypes",
                column: "Id",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_IsActive_IsDeleted",
                table: "TransactionTypes",
                columns: new[] { "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_Name_TenantId",
                table: "TransactionTypes",
                columns: new[] { "Name", "TenantId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_PerformBy",
                table: "TransactionTypes",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_TenantId",
                table: "TransactionTypes",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_TransactionTypes_TransactionTypeId",
                table: "Transactions",
                column: "TransactionTypeId",
                principalTable: "TransactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 7. Drop Old Columns
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Type",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Tier",
                table: "Accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_TransactionTypes_TransactionTypeId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionTypeId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionTypeId",
                table: "Transactions");

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "Transactions",
                type: "TINYINT",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Tier",
                table: "Accounts",
                type: "TINYINT",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                table: "Transactions",
                column: "Type");
        }
    }
}
