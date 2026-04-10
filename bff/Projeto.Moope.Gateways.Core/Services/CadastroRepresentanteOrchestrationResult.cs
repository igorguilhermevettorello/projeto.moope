using Projeto.Moope.Gateways.Core.Models;

namespace Projeto.Moope.Gateways.Core.Services
{
    public sealed class CadastroRepresentanteOrchestrationResult
    {
        public bool Sucesso { get; init; }

        public CadastroRepresentanteCompostoOutput? Dados { get; init; }

        public int StatusCode { get; init; }

        public object? CorpoErro { get; init; }
    }
}
