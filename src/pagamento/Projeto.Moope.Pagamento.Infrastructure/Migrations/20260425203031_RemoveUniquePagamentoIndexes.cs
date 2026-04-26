using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Pagamento.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniquePagamentoIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagamento_ClienteId",
                table: "Pagamento");

            migrationBuilder.DropIndex(
                name: "IX_Pagamento_GatewayCustomerId",
                table: "Pagamento");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_ClienteId",
                table: "Pagamento",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_GatewayCustomerId",
                table: "Pagamento",
                column: "GatewayCustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagamento_ClienteId",
                table: "Pagamento");

            migrationBuilder.DropIndex(
                name: "IX_Pagamento_GatewayCustomerId",
                table: "Pagamento");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_ClienteId",
                table: "Pagamento",
                column: "ClienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_GatewayCustomerId",
                table: "Pagamento",
                column: "GatewayCustomerId",
                unique: true);
        }
    }
}
