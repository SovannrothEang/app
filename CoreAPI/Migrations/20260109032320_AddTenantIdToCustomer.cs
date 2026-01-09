using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Customers",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE c
                SET c.TenantId = u.TenantId
                FROM Customers c
                INNER JOIN Users u ON c.UserId = u.Id
            ");

            // Fallback for any constraints that might fail (e.g. if user not found), though shouldn't happen based on model.
            // If any TenantId is still null, we might have an issue, but we assume integrity of UserId.

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Customers",
                type: "VARCHAR(100)",
                nullable: false,
                defaultValue: "",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Tenants_TenantId",
                table: "Customers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Tenants_TenantId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Customers");
        }
    }
}
