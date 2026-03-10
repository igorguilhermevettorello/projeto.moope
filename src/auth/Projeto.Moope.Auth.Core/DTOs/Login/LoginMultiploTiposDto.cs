using Projeto.Moope.Auth.Core.Enums;

namespace Projeto.Moope.Auth.Core.DTOs.Login
{
    public class LoginMultiploTiposDto
    {
        public string Message { get; set; }
        public IEnumerable<TipoUsuario> TiposUsuario { get; set; }
        public string Email { get; set; }
    }
}
