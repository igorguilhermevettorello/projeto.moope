using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Comodato.Api.DTOs
{
    public class AlterarComodatoDto
    {
        [Required(ErrorMessage = "O campo Id é obrigatório")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O campo ClienteId é obrigatório")]
        public Guid ClienteId { get; set; }

        [Required(ErrorMessage = "O campo ProdutoNome é obrigatório")]
        [MaxLength(120)]
        public string ProdutoNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Valor é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero")]
        public decimal Valor { get; set; }

        public ComodatoStatus Status { get; set; }
    }
}
