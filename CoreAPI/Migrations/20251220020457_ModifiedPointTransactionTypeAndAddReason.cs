using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedPointTransactionTypeAndAddReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Type",
                table: "PointTransactions",
                type: "TINYINT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(100)");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "PointTransactions",
                type: "NVARCHAR(100)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "PointTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "PointTransactions",
                type: "VARCHAR(100)",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "TINYINT");
        }
    }
}
