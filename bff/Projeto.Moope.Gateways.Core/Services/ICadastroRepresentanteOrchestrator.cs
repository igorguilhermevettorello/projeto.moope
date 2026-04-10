using Projeto.Moope.Gateways.Core.Models;

namespace Projeto.Moope.Gateways.Core.Services
{
    public interface ICadastroRepresentanteOrchestrator
    {
        Task<CadastroRepresentanteOrchestrationResult> ExecutarAsync(
            CadastrarRepresentanteInput request,
            string? authorizationHeader,
            CancellationToken cancellationToken);
    }
}
