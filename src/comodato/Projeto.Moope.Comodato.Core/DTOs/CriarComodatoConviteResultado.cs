using Projeto.Moope.Comodato.Core.Models;

namespace Projeto.Moope.Comodato.Core.DTOs
{
    public class CriarComodatoConviteResultado
    {
        public ComodatoConvite Convite { get; set; } = default!;
        public string Token { get; set; } = string.Empty;
    }
}
