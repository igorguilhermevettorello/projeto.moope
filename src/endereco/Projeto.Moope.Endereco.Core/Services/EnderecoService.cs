using Projeto.Moope.Endereco.Core.Interfaces.Repositories;
using Projeto.Moope.Endereco.Core.Interfaces.Services;
using EnderecoModel = Projeto.Moope.Endereco.Core.Models.Endereco;

namespace Projeto.Moope.Endereco.Core.Services
{
    public class EnderecoService : IEnderecoService
    {
        private readonly IEnderecoRepository _enderecoRepository;

        public EnderecoService(IEnderecoRepository enderecoRepository)
        {
            _enderecoRepository = enderecoRepository;
        }

        public async Task<EnderecoModel?> GetById(Guid id)
        {
            return await _enderecoRepository.GetById(id);
        }

        public async Task<IEnumerable<EnderecoModel>> GetAll()
        {
            return await _enderecoRepository.GetAll();
        }

        public async Task Create(EnderecoModel endereco)
        {
            endereco.Created = DateTime.UtcNow;
            endereco.Updated = DateTime.UtcNow;
            await _enderecoRepository.Add(endereco);
        }

        public async Task Update(EnderecoModel endereco)
        {
            endereco.Updated = DateTime.UtcNow;
            await _enderecoRepository.Update(endereco);
        }

        public async Task Delete(Guid id)
        {
            await _enderecoRepository.Delete(id);
        }
    }
}
