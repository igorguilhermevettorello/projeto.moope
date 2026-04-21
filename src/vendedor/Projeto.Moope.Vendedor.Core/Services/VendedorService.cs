using Microsoft.IdentityModel.Tokens;
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

        public async Task<ResultDto<VendedorModel>> SalvarAsync(VendedorModel vendedor)
        {
            var agora = DateTime.UtcNow;
            vendedor.Created = agora;
            vendedor.Updated = agora;

            var entity = await _vendedorRepository.SalvarAsync(vendedor);
            return new ResultDto<VendedorModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<ResultDto<VendedorModel>> AtualizarAsync(VendedorModel vendedor)
        {
            vendedor.Updated = DateTime.UtcNow;

            var entity = await _vendedorRepository.AtualizarAsync(vendedor);
            return new ResultDto<VendedorModel>
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

        public async Task<ResultDto> AtualizarEndereco(Guid vendedorId, Guid enderecoId)
        {
            try
            {
                if (vendedorId == Guid.Empty || enderecoId == Guid.Empty)
                {
                    return new ResultDto { Status = false, Mensagem = "Cliente ou endereço inválidos" };
                }

                var vendedor = await _vendedorRepository.BuscarPorIdAsync(vendedorId);
                if (vendedor == null || vendedor.Id == Guid.Empty)
                {
                    return new ResultDto { Status = false, Mensagem = "Usuário não encontrado" };
                }

                vendedor.Updated = DateTime.UtcNow;
                vendedor.EnderecoId = enderecoId;

                await _vendedorRepository.AtualizarAsync(vendedor);

                _ = await _vendedorRepository.UnitOfWork.Commit();

                return new ResultDto { Status = true };
            }

            catch (Exception)
            {
                return new ResultDto { Status = false, Mensagem = "Erro ao atualizar endereço do usuário" };
            } 
        }
    }
}
