using Projeto.Moope.Pedido.Core.Enums;

namespace Projeto.Moope.Pedido.Api.DTOs
{
    public class PedidoResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid? VendedorId { get; set; }
        public Guid PlanoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PlanoValor { get; set; }
        public string PlanoDescricao { get; set; } = string.Empty;
        public string PlanoCodigo { get; set; } = string.Empty;
        public decimal PlanoTaxaAdesao { get; set; }
        public decimal PlanoPercentualDesconto { get; set; }
        public decimal PlanoValorComDesconto { get; set; }
        public decimal Total { get; set; }
        public StatusAssinatura StatusAssinatura { get; set; }
        public string? Status { get; set; }
        public string? StatusDescricao { get; set; }
        public int? GalaxPayId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public DescontoDto? Desconto { get; set; }
        public List<TransacaoDto>? Transacoes { get; set; }
    }
}

