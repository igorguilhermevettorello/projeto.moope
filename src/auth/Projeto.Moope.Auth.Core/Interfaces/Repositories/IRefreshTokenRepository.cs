using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Core.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        IUnitOfWork UnitOfWork { get; }
        Task<RefreshToken?> BuscarPorTokenAsync(string token);
        Task<RefreshToken> SalvarAsync(RefreshToken entity);
        Task<RefreshToken> AtualizarAsync(RefreshToken entity);
        Task RevogarTokensPorUsuarioAsync(Guid usuarioId);
    }
}
