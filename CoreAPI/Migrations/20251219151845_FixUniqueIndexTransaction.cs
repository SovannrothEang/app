using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueIndexTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_CustomerId_TenantId",
                table: "PointTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CustomerId_TenantId",
                table: "PointTransactions",
                columns: new[] { "CustomerId", "TenantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_CustomerId_TenantId",
                table: "PointTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CustomerId_TenantId",
                table: "PointTransactions",
                columns: new[] { "CustomerId", "TenantId" },
                unique: true);
        }
    }
}
