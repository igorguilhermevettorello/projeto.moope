using Projeto.Moope.Gateways.Core.Models;

namespace Projeto.Moope.Gateways.Core.Services
{
    public interface IProcessarVendaOrchestrator
    {
        Task<ProcessarVendaOrchestrationResult> ExecutarAsync(
            ProcessarVendaInput request,
            ProcessarVendaUsuarioContext? usuario,
            string? authorizationHeader,
            string? idempotencyKey,
            CancellationToken cancellationToken);
    }
}
