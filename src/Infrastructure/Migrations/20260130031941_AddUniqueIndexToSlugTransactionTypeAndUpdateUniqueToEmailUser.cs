using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToSlugTransactionTypeAndUpdateUniqueToEmailUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] <> '' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_TenantId_Slug",
                table: "TransactionTypes",
                columns: new[] { "TenantId", "Slug" },
                unique: true,
                filter: "[Slug] <> '' AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypes_TenantId_Slug",
                table: "TransactionTypes");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] <> '' AND [IsDeleted] = 0");
        }
    }
}
