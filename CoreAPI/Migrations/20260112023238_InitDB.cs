using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    CustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    AccountTypeId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Balance = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => new { x.TenantId, x.CustomerId, x.AccountTypeId });
                });

            migrationBuilder.CreateTable(
                name: "AccountTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(15)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    UserId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    Name = table.Column<string>(type: "VARCHAR(50)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.UniqueConstraint("AK_Roles_Id_TenantId", x => new { x.Id, x.TenantId });
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    Status = table.Column<byte>(type: "TINYINT", nullable: false),
                    Setting_PointPerDollar = table.Column<int>(type: "int", nullable: true),
                    Setting_ExpiryDays = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    FirstName = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    LastName = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    AuthProvider = table.Column<string>(type: "VARCHAR(10)", nullable: false, defaultValue: "Local"),
                    ProviderKey = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: true),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    UserName = table.Column<string>(type: "VARCHAR(50)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "VARCHAR(50)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "VARCHAR(100)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "VARCHAR(100)", maxLength: 256, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.UniqueConstraint("AK_Users_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_PerformBy",
                        column: x => x.PerformBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

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
                    AllowNegative = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
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
            
            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "VARCHAR(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    RoleId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId_TenantId",
                        columns: x => new { x.RoleId, x.TenantId },
                        principalTable: "Roles",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId_TenantId",
                        columns: x => new { x.UserId, x.TenantId },
                        principalTable: "Users",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    TenantId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    CustomerId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    AccountTypeId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,2)", nullable: false),
                    TransactionTypeId = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Reason = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    ReferenceId = table.Column<string>(type: "VARCHAR(100)", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIMEOFFSET(3)", nullable: false),
                    PerformBy = table.Column<string>(type: "VARCHAR(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_TenantId_CustomerId_AccountTypeId",
                        columns: x => new { x.TenantId, x.CustomerId, x.AccountTypeId },
                        principalTable: "Accounts",
                        principalColumns: new[] { "TenantId", "CustomerId", "AccountTypeId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Customers_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionTypes_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_PerformBy",
                        column: x => x.PerformBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountTypeId",
                table: "Accounts",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CustomerId",
                table: "Accounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PerformBy",
                table: "Accounts",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId",
                table: "Accounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId_CustomerId_AccountTypeId",
                table: "Accounts",
                columns: new[] { "TenantId", "CustomerId", "AccountTypeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypes_Id",
                table: "AccountTypes",
                column: "Id",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypes_IsActive_IsDeleted",
                table: "AccountTypes",
                columns: new[] { "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypes_Name_TenantId",
                table: "AccountTypes",
                columns: new[] { "Name", "TenantId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypes_PerformBy",
                table: "AccountTypes",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypes_TenantId",
                table: "AccountTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Id",
                table: "Customers",
                column: "Id",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PerformBy",
                table: "Customers",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PerformBy",
                table: "Roles",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId",
                table: "Roles",
                column: "TenantId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Id",
                table: "Roles",
                columns: new[] { "TenantId", "Id" },
                unique: true,
                filter: "[Id] <> '' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[Name] <> '' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Id",
                table: "Tenants",
                column: "Id",
                unique: true,
                filter: "[IsDeleted] = 0");

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
                name: "IX_Transactions_Id",
                table: "Transactions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PerformBy",
                table: "Transactions",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReferenceId",
                table: "Transactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TenantId_CustomerId_AccountTypeId",
                table: "Transactions",
                columns: new[] { "TenantId", "CustomerId", "AccountTypeId" });

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

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

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
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PerformBy",
                table: "Users",
                column: "PerformBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId",
                filter: "[TenantId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] <> '' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Id",
                table: "Users",
                columns: new[] { "TenantId", "Id" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_UserName",
                table: "Users",
                columns: new[] { "TenantId", "UserName" },
                unique: true,
                filter: "[UserName] <> '' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AccountTypes_AccountTypeId",
                table: "Accounts",
                column: "AccountTypeId",
                principalTable: "AccountTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Customers_CustomerId",
                table: "Accounts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Tenants_TenantId",
                table: "Accounts",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_PerformBy",
                table: "Accounts",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountTypes_Tenants_TenantId",
                table: "AccountTypes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountTypes_Users_PerformBy",
                table: "AccountTypes",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Tenants_TenantId",
                table: "Customers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_PerformBy",
                table: "Customers",
                column: "PerformBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleClaims_Roles_RoleId",
                table: "RoleClaims",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Tenants_TenantId",
                table: "Roles",
                column: "TenantId",
                principalTable: "Tenants",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "AccountTypes");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
