namespace Projeto.Moope.Auth.Core.DTOs.Login
{
    public sealed class ClientLoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public double ExpiresIn { get; set; }
        public IEnumerable<ClaimDto> Claims { get; set; } = [];
        public string Role { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
    }
}

