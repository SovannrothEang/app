using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixedNormalizedNameInRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "Roles");
            
            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_NormalizedName",
                table: "Roles",
                columns: new[] { "TenantId", "NormalizedName" },
                unique: true,
                filter: "[NormalizedName] <> '' AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
            
            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId_NormalizedName",
                table: "Roles");
        }
    }
}
