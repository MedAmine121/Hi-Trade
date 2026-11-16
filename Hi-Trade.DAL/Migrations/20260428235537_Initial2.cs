using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hi_Trade.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Position_Assets_AssetId",
                table: "Position");

            migrationBuilder.DropForeignKey(
                name: "FK_Position_Portfolios_PortfolioId",
                table: "Position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Position",
                table: "Position");

            migrationBuilder.RenameTable(
                name: "Position",
                newName: "Positions");

            migrationBuilder.RenameIndex(
                name: "IX_Position_PortfolioId",
                table: "Positions",
                newName: "IX_Positions_PortfolioId");

            migrationBuilder.RenameIndex(
                name: "IX_Position_AssetId",
                table: "Positions",
                newName: "IX_Positions_AssetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Assets_AssetId",
                table: "Positions",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Assets_AssetId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "Position");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_PortfolioId",
                table: "Position",
                newName: "IX_Position_PortfolioId");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_AssetId",
                table: "Position",
                newName: "IX_Position_AssetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Position",
                table: "Position",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Position_Assets_AssetId",
                table: "Position",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Position_Portfolios_PortfolioId",
                table: "Position",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
