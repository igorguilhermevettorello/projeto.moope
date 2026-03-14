using Projeto.Moope.Core.DTOs;
using ComodatoModel = Projeto.Moope.Comodato.Core.Models.Comodato;

namespace Projeto.Moope.Comodato.Core.Interfaces.Services
{
    public interface IComodatoService
    {
        Task<ComodatoModel?> BuscarPorIdAsync(Guid id);
        Task<ComodatoModel?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<IEnumerable<ComodatoModel>> BuscarTodosAsync();
        Task<IEnumerable<ComodatoModel>> BuscarPorClienteIdAsync(Guid clienteId);
        Task<Result<ComodatoModel>> SalvarAsync(ComodatoModel comodato);
        Task<Result<ComodatoModel>> AtualizarAsync(ComodatoModel comodato);
        Task<Result<ComodatoModel>> AlterarStatusAsync(ComodatoModel comodato, Projeto.Moope.Core.Enums.ComodatoStatus status);
        Task<bool> RemoverAsync(Guid id);
    }
}
