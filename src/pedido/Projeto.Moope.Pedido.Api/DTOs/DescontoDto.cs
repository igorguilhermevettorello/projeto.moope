using Projeto.Moope.Pedido.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Pedido.Api.DTOs
{
    public class DescontoDto
    {
        [Required(ErrorMessage = "O percentual de desconto é obrigatório")]
        [Range(0, 100, ErrorMessage = "O percentual deve estar entre 0 e 100")]
        public decimal ValorPercentual { get; set; }

        [Required(ErrorMessage = "A descrição do desconto é obrigatória")]
        [MaxLength(255)]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo pessoa é obrigatório")]
        public TipoPessoaDesconto TipoPessoa { get; set; }

        [Required(ErrorMessage = "O código do desconto é obrigatório")]
        [MaxLength(100)]
        public string CodigoDesconto { get; set; } = string.Empty;

        public decimal? ValorDesconto { get; set; }
        public bool Ativo { get; set; } = true;
    }
}

