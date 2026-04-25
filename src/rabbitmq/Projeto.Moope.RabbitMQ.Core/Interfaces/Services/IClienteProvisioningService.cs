using Projeto.Moope.Core.DTOs;

namespace Projeto.Moope.RabbitMQ.Core.Interfaces.Services
{
    public interface IClienteProvisioningService
    {
        Task<ResultDto> CriarClienteSeNaoExistirAsync(
            Guid usuarioId,
            string email,
            string authorizationHeader,
            CancellationToken cancellationToken);
    }
}

