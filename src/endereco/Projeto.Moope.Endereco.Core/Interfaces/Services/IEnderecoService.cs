using EnderecoModel = Projeto.Moope.Endereco.Core.Models.Endereco;

namespace Projeto.Moope.Endereco.Core.Interfaces.Services
{
    public interface IEnderecoService
    {
        Task<EnderecoModel?> GetById(Guid id);
        Task<IEnumerable<EnderecoModel>> GetAll();
        Task Create(EnderecoModel endereco);
        Task Update(EnderecoModel endereco);
        Task Delete(Guid id);
    }
}