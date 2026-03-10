using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Core.Interfaces.Repositories
{
    public interface IPessoaFisicaRepository : IRepository<PessoaFisica>
    {
        Task<PessoaFisica> BuscarPorCpfAsync(string cpf);
        Task<PessoaFisica> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}
