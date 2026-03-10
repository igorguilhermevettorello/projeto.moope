using EnderecoModel = Projeto.Moope.Endereco.Core.Models.Endereco;

namespace Projeto.Moope.Endereco.Core.Interfaces.Repositories
{
    public interface IEnderecoRepository
    {
        Task<EnderecoModel?> GetById(Guid id);
        Task<IEnumerable<EnderecoModel>> GetAll();
        Task Add(EnderecoModel endereco);
        Task Update(EnderecoModel endereco);
        Task Delete(Guid id);
    }
}