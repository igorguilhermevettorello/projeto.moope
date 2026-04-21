using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Pedido.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidoTipoPessoaEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Pedido",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TipoPessoa",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TipoPessoa",
                table: "Pedido");
        }
    }
}
