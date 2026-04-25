using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs.Auth;

namespace Projeto.Moope.RabbitMQ.Core.Interfaces.Services
{
    public interface IAuthClientTokenService
    {
        Task<ResultDto<ClientAccessTokenDto>> GetClientAccessTokenAsync(CancellationToken cancellationToken);
    }
}

