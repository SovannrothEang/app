using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CustomerId_OccurredAt",
                table: "Transactions",
                columns: new[] { "CustomerId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OccurredAt",
                table: "Transactions",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TenantId_OccurredAt",
                table: "Transactions",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CreatedAt",
                table: "Tenants",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Status",
                table: "Tenants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedAt",
                table: "Customers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Balance",
                table: "Accounts",
                column: "Balance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CustomerId_OccurredAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_OccurredAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TenantId_OccurredAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_CreatedAt",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Name",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Status",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CreatedAt",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Balance",
                table: "Accounts");
        }
    }
}
