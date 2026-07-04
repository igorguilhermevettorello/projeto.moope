using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Comodato.Api.DTOs
{
    public class ComodatoConviteResponseDto
    {
        public Guid Id { get; set; }
        public string? Token { get; set; }
        public Guid PlanoId { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }
        public Guid? VendedorId { get; set; }
        public string? Estado { get; set; }
        public DateTime? DataPagamento { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime ExpiradoEm { get; set; }
        public ComodatoConviteStatus Status { get; set; }
        public Guid CreatedByAdminId { get; set; }
    }
}
