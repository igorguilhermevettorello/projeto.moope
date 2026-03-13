using Projeto.Moope.Core.DTOs;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Core.Interfaces.Services
{
    public interface IPlanoService
    {
        Task<PlanoModel?> BuscarPorIdAsync(Guid id);
        Task<PlanoModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<PlanoModel?> BuscarPorPlanoSelecionadoAsync(string codigo);
        Task<IEnumerable<PlanoModel>> BuscarTodosAsync();
        Task<Result<PlanoModel>> SalvarAsync(PlanoModel plano);
        Task<Result<PlanoModel>> AtualizarAsync(PlanoModel plano);
        Task<Result<PlanoModel>> AtivarInativarAsync(PlanoModel plano, bool status);
        Task<bool> RemoverAsync(Guid id);
    }
}
