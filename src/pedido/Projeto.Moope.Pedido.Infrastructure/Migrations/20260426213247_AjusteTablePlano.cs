using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Pedido.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteTablePlano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Total",
                table: "Pedido",
                newName: "PlanoValorTotal");

            migrationBuilder.RenameColumn(
                name: "PlanoValorComDesconto",
                table: "Pedido",
                newName: "PlanoTaxaAdesaoTotal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlanoValorTotal",
                table: "Pedido",
                newName: "Total");

            migrationBuilder.RenameColumn(
                name: "PlanoTaxaAdesaoTotal",
                table: "Pedido",
                newName: "PlanoValorComDesconto");
        }
    }
}
