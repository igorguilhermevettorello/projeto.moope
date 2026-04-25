using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs.Usuario;

namespace Projeto.Moope.RabbitMQ.Core.Interfaces.Services
{
    public interface IClientesPendentesQueryService
    {
        Task<ResultDto<IReadOnlyList<ClientePendenteDto>>> ListarClientesPendentesAsync(
            string authorizationHeader,
            CancellationToken cancellationToken);
    }
}

