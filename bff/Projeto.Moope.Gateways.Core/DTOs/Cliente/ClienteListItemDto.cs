namespace Projeto.Moope.Gateways.Core.DTOs.Cliente
{
    public class ClienteListItemDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Cpf { get; set; }
        public string? Cnpj { get; set; }
        public string? TipoPessoa { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Telefone { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public bool Ativo { get; set; }
    }
}
