using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Plano.Api.DTOs
{
    public class CriarPlanoDto
    {
        [Required(ErrorMessage = "O campo Código é obrigatório")]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Descrição é obrigatório")]
        [MaxLength(255)]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Valor é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero")]
        public decimal Valor { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A taxa de adesão deve ser maior ou igual a zero")]
        public decimal? TaxaAdesao { get; set; }

        public bool Status { get; set; } = true;

        public bool Plataforma { get; set; }
    }
}
