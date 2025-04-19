using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gringotts.LedgerService.Migrations
{
    /// <inheritdoc />
    public partial class AddSickleAndKnutRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KnutToDkk",
                table: "ExchangeRates",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SickleToDkk",
                table: "ExchangeRates",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KnutToDkk",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "SickleToDkk",
                table: "ExchangeRates");
        }
    }
}
