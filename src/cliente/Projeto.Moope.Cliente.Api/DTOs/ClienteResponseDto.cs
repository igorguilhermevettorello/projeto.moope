using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Cliente.Api.DTOs
{
    public class ClienteResponseDto
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
