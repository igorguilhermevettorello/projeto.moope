using Projeto.Moope.Pedido.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Pedido.Api.DTOs
{
    public class TransacaoDto
    {
        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data de pagamento é obrigatória")]
        public DateTime DataPagamento { get; set; }

        [Required(ErrorMessage = "O status do pagamento é obrigatório")]
        public StatusPagamento StatusPagamento { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(255)]
        public string? StatusDescricao { get; set; }

        public int? GalaxPayId { get; set; }

        [Required(ErrorMessage = "O método de pagamento é obrigatório")]
        [MaxLength(50)]
        public string MetodoPagamento { get; set; } = string.Empty;
    }
}

