using Projeto.Moope.Comodato.Core.DTOs;
using Projeto.Moope.Comodato.Core.Models;
using Projeto.Moope.Core.DTOs;

namespace Projeto.Moope.Comodato.Core.Interfaces.Services
{
    public interface IComodatoConviteService
    {
        Task<ComodatoConvite?> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<ResultDto<CriarComodatoConviteResultado>> CriarAsync(CriarComodatoConviteInput input, Guid adminId);
    }
}
