using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Auth.Core.DTOs.Usuario
{
    public class AlterarSenhaUsuarioDto
    {
        [Required(ErrorMessage = "O campo ID do usuário é obrigatório")]
        public Guid UsuarioId { get; set; }

        [Required(ErrorMessage = "O campo Senha Atual é obrigatório")]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Nova Senha é obrigatório")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres")]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Confirmação da Nova Senha é obrigatório")]
        [Compare("NovaSenha", ErrorMessage = "A confirmação deve ser igual à nova senha")]
        public string ConfirmacaoNovaSenha { get; set; } = string.Empty;
    }
}
