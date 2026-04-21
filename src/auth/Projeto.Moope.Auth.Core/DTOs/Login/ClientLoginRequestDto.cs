namespace Projeto.Moope.Auth.Core.DTOs.Login
{
    public sealed class ClientLoginRequestDto
    {
        public string ClienteId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }
}

