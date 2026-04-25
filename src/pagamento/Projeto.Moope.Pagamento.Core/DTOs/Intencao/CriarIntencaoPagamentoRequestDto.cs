using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Pagamento.Core.DTOs.Intencao
{
    public class CriarIntencaoPagamentoRequestDto
    {
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Moeda é obrigatória.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Moeda deve ter 3 caracteres (ex.: BRL).")]
        public string Moeda { get; set; } = string.Empty;

        [Required(ErrorMessage = "Método de pagamento é obrigatório.")]
        public MetodoPagamento MetodoPagamento { get; set; }
    }
}
