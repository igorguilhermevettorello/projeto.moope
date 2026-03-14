using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Comodato.Api.DTOs
{
    public class ComodatoResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime CriadoEm { get; set; }
        public ComodatoStatus Status { get; set; }
    }
}
