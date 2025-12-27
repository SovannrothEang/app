using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDateTimeToDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users");
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_UserName",
                table: "Users");
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Id",
                table: "Users");
            
            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId",
                table: "Roles");
            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId_Id",
                table: "Roles");
            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles");
            
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Users",
                type: "DATETIMEOFFSET(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "BIT",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "BIT",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Users",
                type: "DATETIMEOFFSET(3)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Users",
                type: "DATETIMEOFFSET(3)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "OccurredAt",
                table: "Transactions",
                type: "DATETIMEOFFSET(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2(3)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Transactions",
                type: "DATETIMEOFFSET(3)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Tenants",
                type: "DATETIMEOFFSET(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Tenants",
                type: "DATETIMEOFFSET(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2(3)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Tenants",
                type: "DATETIMEOFFSET(3)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Roles",
                type: "DATETIMEOFFSET(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Roles",
                type: "BIT",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Roles",
                type: "DATETIMEOFFSET(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Roles",
                type: "DATETIMEOFFSET(3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "BIT",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Customers",
                type: "DATETIMEOFFSET(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Customers",
                type: "DATETIMEOFFSET(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2(3)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Customers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Accounts",
                type: "DATETIMEOFFSET(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Accounts",
                type: "BIT",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "BIT",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Accounts",
                type: "DATETIMEOFFSET(3)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Accounts",
                type: "DATETIMEOFFSET(3)",
                nullable: true);
            
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId",
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] <> '' AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_UserName",
                table: "Users",
                columns: new[] { "TenantId", "UserName" },
                unique: true,
                filter: "[UserName] <> '' AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Id",
                table: "Users",
                columns: new[] { "TenantId", "Id" },
                unique: true,
                filter: "[TenantId] <> '' AND [IsDeleted] = 0");
            
            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId",
                table: "Roles",
                column: "TenantId",
                filter: "[TenantId] IS NOT NULL AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[Name] <> '' AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Id",
                table: "Roles",
                columns: new[] { "TenantId", "Id" },
                unique: true,
                filter: "[Id] <> '' AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Accounts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BIT",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BIT",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OccurredAt",
                table: "Transactions",
                type: "DATETIME2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tenants",
                type: "DATETIME2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tenants",
                type: "DATETIME2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Roles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Roles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BIT",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Customers",
                type: "DATETIME2(3)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Customers",
                type: "DATETIME2(3)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Accounts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Accounts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BIT",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BIT",
                oldDefaultValue: true);

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users");
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_UserName",
                table: "Users");
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Id",
                table: "Users");
            
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId",
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] <> '' AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_UserName",
                table: "Users",
                columns: new[] { "TenantId", "UserName" },
                unique: true,
                filter: "[UserName] <> '' AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Id",
                table: "Users",
                columns: new[] { "TenantId", "Id" },
                unique: true,
                filter: "[TenantId] <> '' AND [IsDeleted] = 0");
            
            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId",
                table: "Roles",
                column: "TenantId",
                filter: "[TenantId] IS NOT NULL AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[Name] <> '' AND [IsDeleted] = 0");
            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Id",
                table: "Roles",
                columns: new[] { "TenantId", "Id" },
                unique: true,
                filter: "[Id] <> '' AND [IsDeleted] = 0");
        }
    }
}
