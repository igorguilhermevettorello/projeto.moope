using Projeto.Moope.Gateways.Core.Models;

namespace Projeto.Moope.Gateways.Core.Services
{
    public interface ICadastroClienteOrchestrator
    {
        Task<CadastroClienteOrchestrationResult> ExecutarAsync(
            CadastrarClienteInput request,
            string? authorizationHeader,
            CancellationToken cancellationToken);
    }
}
