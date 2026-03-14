using Projeto.Moope.Core.Interfaces.Data;
using ComodatoModel = Projeto.Moope.Comodato.Core.Models.Comodato;

namespace Projeto.Moope.Comodato.Core.Interfaces.Repositories
{
    public interface IComodatoRepository : IRepository<ComodatoModel>
    {
        Task<ComodatoModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<IEnumerable<ComodatoModel>> BuscarPorClienteIdAsync(Guid clienteId);
    }
}
