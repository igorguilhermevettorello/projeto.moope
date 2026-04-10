using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Vendedor.Api.DTOs
{
    public class VendedorResponseDto
    {
        public Guid Id { get; set; }

        public TipoPessoa TipoPessoa { get; set; }

        public string CpfCnpj { get; set; } = string.Empty;

        public decimal PercentualComissao { get; set; }

        public string ChavePix { get; set; } = string.Empty;

        public string CodigoCupom { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public Guid? VendedorId { get; set; }
    }
}
