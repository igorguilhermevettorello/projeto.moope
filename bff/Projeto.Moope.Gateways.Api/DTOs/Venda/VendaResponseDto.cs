namespace Projeto.Moope.Gateways.Api.DTOs.Venda
{
    public sealed class VendaResponseDto
    {
        public Guid PlanoId { get; set; }

        public int Quantidade { get; set; }

        public decimal ValorTotal { get; set; }

        public Guid VendaId { get; set; }

        public Guid TransacaoId { get; set; }
    }
}
