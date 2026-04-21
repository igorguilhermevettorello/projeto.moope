using Projeto.Moope.Core.DTOs;

namespace Projeto.Moope.Gateways.Core.Interfaces.Services
{
    public interface IAuthClientLoginService
    {
        Task<ResultDto<string>> ExecutarAsync(CancellationToken cancellationToken);
    }
}
