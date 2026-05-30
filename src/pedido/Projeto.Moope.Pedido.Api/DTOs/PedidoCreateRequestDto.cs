using Projeto.Moope.Core.Enums;
using Projeto.Moope.Pedido.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Pedido.Api.DTOs
{
    public class PedidoCreateRequestDto
    {
        [Required(ErrorMessage = "O cliente é obrigatório")]
        public Guid ClienteId { get; set; }

        public Guid? VendedorId { get; set; }

        [Required(ErrorMessage = "O plano é obrigatório")]
        public Guid PlanoId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "O campo TipoPessoa e obrigatorio")]
        public TipoPessoa TipoPessoa { get; set; }

        public string? Estado { get; set; }

        public string? TipoPlataforma { get; set; }

        public bool? Rastreamento { get; set; }

        //[Required(ErrorMessage = "O valor do plano é obrigatório")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "O valor do plano deve ser maior que zero")]
        //public decimal PlanoValor { get; set; }

        //[Required(ErrorMessage = "A descrição do plano é obrigatória")]
        //[MaxLength(255)]
        //public string PlanoDescricao { get; set; } = string.Empty;

        //[Required(ErrorMessage = "O código do plano é obrigatório")]
        //[MaxLength(50)]
        //public string PlanoCodigo { get; set; } = string.Empty;

        //[Required(ErrorMessage = "A taxa de adesão é obrigatória")]
        //public decimal PlanoTaxaAdesao { get; set; }

        //[Required(ErrorMessage = "O percentual de desconto do plano é obrigatório")]
        //[Range(0, 100, ErrorMessage = "O percentual deve estar entre 0 e 100")]
        //public decimal PlanoPercentualDesconto { get; set; }

        //[Required(ErrorMessage = "O valor do plano com desconto é obrigatório")]
        //public decimal PlanoValorComDesconto { get; set; }

        //[Required(ErrorMessage = "O total é obrigatório")]
        //public decimal Total { get; set; }

        //[Required(ErrorMessage = "O status da assinatura é obrigatório")]
        //public StatusAssinatura StatusAssinatura { get; set; }

        //[MaxLength(50)]
        //public string? Status { get; set; }

        //[MaxLength(255)]
        //public string? StatusDescricao { get; set; }

        //public int? GalaxPayId { get; set; }

        //public DescontoDto? Desconto { get; set; }

        //public List<TransacaoDto>? Transacoes { get; set; }
    }
}

