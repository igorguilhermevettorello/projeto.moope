using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Core.Interfaces.Repositories
{
    public interface IPessoaJuridicaRepository : IRepository<PessoaJuridica>
    {
        Task<PessoaJuridica> BuscarPorCnpjAsync(string cnpj);
        Task<PessoaJuridica> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}
