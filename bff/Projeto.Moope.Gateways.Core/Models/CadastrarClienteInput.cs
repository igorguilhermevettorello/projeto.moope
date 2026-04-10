using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Gateways.Core.Models
{
    public sealed class CadastrarClienteInput
    {
        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public TipoPessoa TipoPessoa { get; set; }

        public string CpfCnpj { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public RepresentanteEnderecoInput Endereco { get; set; } = null!;

        public string Senha { get; set; } = string.Empty;

        public string Confirmacao { get; set; } = string.Empty;

        public string NomeFantasia { get; set; } = string.Empty;

        public string InscricaoEstadual { get; set; } = string.Empty;

        public Guid? VendedorId { get; set; }
    }
}
