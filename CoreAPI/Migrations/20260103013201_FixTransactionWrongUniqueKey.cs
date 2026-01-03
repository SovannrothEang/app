using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTransactionWrongUniqueKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_TenantId_CustomerId",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TenantId_CustomerId",
                table: "Transactions",
                columns: new[] { "TenantId", "CustomerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_TenantId_CustomerId",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TenantId_CustomerId",
                table: "Transactions",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true);
        }
    }
}
