using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAllowNegativeForTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowNegative",
                table: "TransactionTypes",
                type: "BIT",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql(@"
                UPDATE TransactionTypes
                SET AllowNegative = CASE
                    WHEN Slug = 'earn' THEN 0
                    WHEN Slug = 'adjust' THEN 1
                    WHEN Slug = 'redeem' THEN 0
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowNegative",
                table: "TransactionTypes");
        }
    }
}
