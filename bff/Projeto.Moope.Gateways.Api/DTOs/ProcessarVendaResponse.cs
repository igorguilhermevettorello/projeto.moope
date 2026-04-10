namespace Projeto.Moope.Gateways.Api.DTOs
{
    public sealed class ProcessarVendaResponse
    {
        public Guid PlanoId { get; set; }

        public int Quantidade { get; set; }

        public decimal ValorTotal { get; set; }

        public Guid VendaId { get; set; }

        public Guid TransacaoId { get; set; }
    }
}
