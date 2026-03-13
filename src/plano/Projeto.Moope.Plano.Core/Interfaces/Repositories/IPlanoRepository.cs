using Projeto.Moope.Core.Interfaces.Data;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Core.Interfaces.Repositories
{
    public interface IPlanoRepository : IRepository<PlanoModel>
    {
        Task<PlanoModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<PlanoModel?> BuscarPorPlanoSelecionadoAsync(string codigo);
    }
}
