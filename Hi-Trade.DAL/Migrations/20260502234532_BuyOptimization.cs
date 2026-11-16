using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hi_Trade.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BuyOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Portfolios");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Positions",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Positions",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Portfolios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
