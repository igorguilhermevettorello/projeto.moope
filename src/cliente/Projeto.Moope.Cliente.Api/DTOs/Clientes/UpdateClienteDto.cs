using Projeto.Moope.Api.Attributes;
using Projeto.Moope.Cliente.Api.DTOs.Enderecos;
using Projeto.Moope.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Cliente.Api.DTOs.Clientes
{
    public class UpdateClienteDto
    {
        [Required(ErrorMessage = "O campo Id é obrigatório")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        [Documento("TipoPessoa")]
        public string CpfCnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo TipoPessoa é obrigatório")]
        public TipoPessoa TipoPessoa { get; set; }

        public bool Ativo { get; set; } = true;

        [Required(ErrorMessage = "O campo Endereco é obrigatório")]
        public UpdateEnderecoDto? Endereco { get; set; }

        public string NomeFantasia { get; set; } = string.Empty;
        public string InscricaoEstadual { get; set; } = string.Empty;
        public Guid? VendedorId { get; set; }
    }
}
