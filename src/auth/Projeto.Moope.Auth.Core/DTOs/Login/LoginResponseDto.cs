namespace Projeto.Moope.Auth.Core.DTOs.Login
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public double ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public double RefreshTokenExpiresIn { get; set; }
        public LoginUsuarioDto User { get; set; } = null!;
    }
}
