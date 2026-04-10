using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Gateways.Api.DTOs
{
    public class RepresentanteEnderecoRequest
    {
        [Required(ErrorMessage = "O campo CEP é obrigatório")]
        public string Cep { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Logradouro é obrigatório")]
        public string Logradouro { get; set; } = string.Empty;

        public string? Numero { get; set; }

        public string? Complemento { get; set; }

        [Required(ErrorMessage = "O campo Bairro é obrigatório")]
        public string Bairro { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Cidade é obrigatório")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Estado é obrigatório")]
        public string Estado { get; set; } = string.Empty;
    }
}
