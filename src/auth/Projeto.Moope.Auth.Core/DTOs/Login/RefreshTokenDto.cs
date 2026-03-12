using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Auth.Core.DTOs.Login
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "O Refresh Token é obrigatório")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
