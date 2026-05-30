using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Pedido.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustePedidoPlataforma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Rastreamento",
                table: "Pedido",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPlataforma",
                table: "Pedido",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rastreamento",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TipoPlataforma",
                table: "Pedido");
        }
    }
}
