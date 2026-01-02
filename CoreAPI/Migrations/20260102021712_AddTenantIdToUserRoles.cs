using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "VARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                table: "Users",
                type: "VARCHAR(10)",
                nullable: false,
                defaultValue: "Local");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "VARCHAR(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "VARCHAR(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PerformBy",
                table: "Users",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderKey",
                table: "Users",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "UserRoles",
                type: "VARCHAR(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                    UPDATE UserRoles
                    SET TenantId = r.TenantId
                    FROM UserRoles ur
                    INNER JOIN Roles r ON ur.RoleId = r.Id
                """);

            migrationBuilder.AddColumn<string>(
                name: "PerformBy",
                table: "Transactions",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformBy",
                table: "Tenants",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Tenants",
                type: "VARCHAR(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PerformBy",
                table: "Roles",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformBy",
                table: "Customers",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "Accounts",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INT");

            migrationBuilder.AddColumn<string>(
                name: "PerformBy",
                table: "Accounts",
                type: "VARCHAR(100)",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Id_TenantId",
                table: "Users",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Roles_Id_TenantId",
                table: "Roles",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PerformBy",
                table: "Users",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId_TenantId",
                table: "UserRoles",
                columns: new[] { "RoleId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_TenantId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_TenantId",
                table: "UserRoles",
                columns: new[] { "UserId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PerformBy",
                table: "Transactions",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PerformBy",
                table: "Tenants",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true,
                filter: "[Slug] <> '' AND  [Slug] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PerformBy",
                table: "Roles",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PerformBy",
                table: "Customers",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PerformBy",
                table: "Accounts",
                column: "PerformBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_PerformBy",
                table: "Accounts",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_PerformBy",
                table: "Customers",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_PerformBy",
                table: "Roles",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Users_PerformBy",
                table: "Tenants",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_PerformBy",
                table: "Transactions",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId_TenantId",
                table: "UserRoles",
                columns: new[] { "RoleId", "TenantId" },
                principalTable: "Roles",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId_TenantId",
                table: "UserRoles",
                columns: new[] { "UserId", "TenantId" },
                principalTable: "Users",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_PerformBy",
                table: "Users",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_PerformBy",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_PerformBy",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_PerformBy",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Users_PerformBy",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_PerformBy",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId_TenantId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId_TenantId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_PerformBy",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Id_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PerformBy",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RoleId_TenantId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId_TenantId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_TenantId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PerformBy",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_PerformBy",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Roles_Id_TenantId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_PerformBy",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PerformBy",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_PerformBy",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AuthProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PerformBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProviderKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "PerformBy",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PerformBy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PerformBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "PerformBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PerformBy",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "VARCHAR(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Customers",
                type: "NVARCHAR(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "VARCHAR(25)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "Accounts",
                type: "INT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true,
                filter: "[Email] <> '' AND [IsDeleted] = 0");
        }
    }
}
