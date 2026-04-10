using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Vendedor.Api.DTOs
{
    public class AlterarVendedorDto
    {
        [Required(ErrorMessage = "O campo Id é obrigatório")]
        public Guid Id { get; set; }

        public TipoPessoa TipoPessoa { get; set; }

        [MaxLength(18)]
        public string CpfCnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "O percentual de comissão é obrigatório")]
        [Range(0, 100, ErrorMessage = "O percentual deve estar entre 0 e 100")]
        public decimal PercentualComissao { get; set; }

        [Required(ErrorMessage = "A chave Pix é obrigatória")]
        [MaxLength(255)]
        public string ChavePix { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código do cupom é obrigatório")]
        [MaxLength(100)]
        public string CodigoCupom { get; set; } = string.Empty;

        public Guid? VendedorId { get; set; }
    }
}
