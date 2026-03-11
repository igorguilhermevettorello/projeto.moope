using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Core.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<Dictionary<Guid, string>> BuscarNomesPorIdsAsync(IEnumerable<Guid> ids);
    }
}
