using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Cliente.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteColunaGalaxPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GalaxPayId",
                table: "Cliente",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GalaxPayId",
                table: "Cliente");
        }
    }
}
