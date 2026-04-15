using System;

namespace Projeto.Moope.Gateways.Core.Models
{
    [Obsolete("Use CadastroClienteCompostoOutput")]
    public sealed class CadastrarClienteOutput
    {
        public Guid ClienteId { get; init; }
    }
}
