namespace Projeto.Moope.Auth.Core.DTOs.Login
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public double ExpiresIn { get; set; }
        public LoginUsuarioDto User { get; set; }
    }
}
