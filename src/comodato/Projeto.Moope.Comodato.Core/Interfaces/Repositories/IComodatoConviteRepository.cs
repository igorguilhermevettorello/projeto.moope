using Projeto.Moope.Comodato.Core.Models;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Comodato.Core.Interfaces.Repositories
{
    public interface IComodatoConviteRepository : IRepository<ComodatoConvite>
    {
        Task<ComodatoConvite?> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}
