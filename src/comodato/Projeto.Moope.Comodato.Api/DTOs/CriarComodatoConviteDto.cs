using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Comodato.Api.DTOs
{
    public class CriarComodatoConviteDto
    {
        [Required(ErrorMessage = "O campo Quantidade é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "O campo VendedorId é obrigatório")]
        public Guid VendedorId { get; set; }

        [Required(ErrorMessage = "O campo PlanoId é obrigatório")]
        public Guid PlanoId { get; set; }

        [Required(ErrorMessage = "O campo Valor é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "O campo Estado é obrigatório")]
        [MaxLength(50, ErrorMessage = "O Estado deve ter no máximo 50 caracteres")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo DataPagamento é obrigatório")]
        public DateTime DataPagamento { get; set; }
    }
}
