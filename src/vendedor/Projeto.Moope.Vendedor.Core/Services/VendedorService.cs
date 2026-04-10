using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Vendedor.Core.Interfaces.Repositories;
using Projeto.Moope.Vendedor.Core.Interfaces.Services;
using VendedorModel = Projeto.Moope.Vendedor.Core.Models.Vendedor;

namespace Projeto.Moope.Vendedor.Core.Services
{
    public class VendedorService : BaseService, IVendedorService
    {
        private readonly IVendedorRepository _vendedorRepository;

        public VendedorService(
            IVendedorRepository vendedorRepository,
            INotificador notificador) : base(notificador)
        {
            _vendedorRepository = vendedorRepository;
        }

        public async Task<VendedorModel?> BuscarPorIdAsync(Guid id)
        {
            return await _vendedorRepository.BuscarPorIdAsync(id);
        }

        public async Task<VendedorModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _vendedorRepository.BuscarPorIdAsNotrackingAsync(id);
        }

        public async Task<IEnumerable<T>> BuscarVendedoresComDadosAsync<T>()
        {
            return await _vendedorRepository.BuscarVendedoresComDadosAsync<T>();
        }

        public async Task<T?> BuscarVendedorPorIdComDadosAsync<T>(Guid id)
        {
            return await _vendedorRepository.BuscarVendedorPorIdComDadosAsync<T>(id);
        }

        public async Task<VendedorModel?> BuscarPorCodigoCupomAsync(string codigoCupom)
        {
            return await _vendedorRepository.BuscarPorCodigoCupomAsync(codigoCupom);
        }

        public async Task<IEnumerable<VendedorModel>> BuscarTodosAsync()
        {
            return await _vendedorRepository.BuscarTodosAsync();
        }

        public async Task<Result<VendedorModel>> SalvarAsync(VendedorModel vendedor)
        {
            var agora = DateTime.UtcNow;
            vendedor.Created = agora;
            vendedor.Updated = agora;

            var entity = await _vendedorRepository.SalvarAsync(vendedor);
            return new Result<VendedorModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<VendedorModel>> AtualizarAsync(VendedorModel vendedor)
        {
            vendedor.Updated = DateTime.UtcNow;

            var entity = await _vendedorRepository.AtualizarAsync(vendedor);
            return new Result<VendedorModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _vendedorRepository.RemoverAsync(id);
            return true;
        }
    }
}
