using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gringotts.LedgerService.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Balances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DkkAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Galleons = table.Column<int>(type: "integer", nullable: false),
                    Sickles = table.Column<int>(type: "integer", nullable: false),
                    Knuts = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Balances_UserId",
                table: "Balances",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Balances");
        }
    }
}
