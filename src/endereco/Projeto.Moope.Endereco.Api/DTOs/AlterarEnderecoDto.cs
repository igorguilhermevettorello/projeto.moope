using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Endereco.Api.DTOs
{
    public class AlterarEnderecoDto
    {
        [Required(ErrorMessage = "O campo ID é obrigatório")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "O campo CEP é obrigatório")]
        public string Cep { get; set; }
        [Required(ErrorMessage = "O campo Logradouro é obrigatório")]
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        [Required(ErrorMessage = "O campo Bairro é obrigatório")]
        public string Bairro { get; set; }
        [Required(ErrorMessage = "O campo Cidade é obrigatório")]
        public string Cidade { get; set; }
        [Required(ErrorMessage = "O campo Estado é obrigatório")]
        public string Estado { get; set; }
    }
}
