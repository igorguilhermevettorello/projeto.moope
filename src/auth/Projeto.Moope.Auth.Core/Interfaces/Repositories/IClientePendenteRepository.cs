using Projeto.Moope.Auth.Core.DTOs.Usuario;

namespace Projeto.Moope.Auth.Core.Interfaces.Repositories
{
    public interface IClientePendenteRepository
    {
        Task<IReadOnlyList<ClientePendenteDto>> ListarClientesPendentesAsync(CancellationToken cancellationToken = default);
    }
}

