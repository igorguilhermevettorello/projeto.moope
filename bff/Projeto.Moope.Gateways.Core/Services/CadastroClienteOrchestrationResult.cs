using Projeto.Moope.Gateways.Core.Models;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class CadastroClienteOrchestrationResult
    {
        public bool Sucesso { get; init; }

        public CadastroClienteCompostoOutput? Dados { get; init; }

        public int StatusCode { get; init; }

        public object? CorpoErro { get; init; }
    }
}
