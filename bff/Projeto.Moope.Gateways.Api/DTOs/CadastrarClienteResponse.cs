using System;

namespace Projeto.Moope.Gateways.Api.DTOs
{
    [Obsolete("Use CadastroClienteCompostoResponse")]
    public sealed class CadastrarClienteResponse
    {
        public Guid ClienteId { get; set; }
    }
}
