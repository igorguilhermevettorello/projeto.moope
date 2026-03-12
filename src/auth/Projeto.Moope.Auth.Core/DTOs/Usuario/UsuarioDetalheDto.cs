using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Auth.Core.DTOs.Usuario
{
    public class UsuarioDetalheDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public TipoPessoa TipoPessoa { get; set; }
        public string? NomeFantasia { get; set; }
        public string? InscricaoEstadual { get; set; }
        public Guid? VendedorId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
