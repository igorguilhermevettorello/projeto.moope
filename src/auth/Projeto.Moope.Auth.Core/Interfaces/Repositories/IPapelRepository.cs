using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Core.Interfaces.Repositories
{
    public interface IPapelRepository : IRepository<Papel>
    {
        Task<IEnumerable<Papel>> BuscarPorUsuarioIdAsync(Guid usuarioId);
    }
}
