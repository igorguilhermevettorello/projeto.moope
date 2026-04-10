namespace Projeto.Moope.Gateways.Core.Models
{
    public sealed class RepresentanteEnderecoInput
    {
        public string Cep { get; set; } = string.Empty;

        public string Logradouro { get; set; } = string.Empty;

        public string? Numero { get; set; }

        public string? Complemento { get; set; }

        public string Bairro { get; set; } = string.Empty;

        public string Cidade { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
    }
}
