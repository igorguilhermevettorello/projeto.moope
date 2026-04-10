namespace Projeto.Moope.Gateways.Core.Models
{
    public sealed class ProcessarVendaOrchestrationResult
    {
        public bool Sucesso { get; init; }

        public int StatusCode { get; init; }

        public object? CorpoErro { get; init; }

        public ProcessarVendaOutput? Dados { get; init; }
    }
}
