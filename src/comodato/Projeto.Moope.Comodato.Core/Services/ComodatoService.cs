using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Comodato.Core.Interfaces.Repositories;
using Projeto.Moope.Comodato.Core.Interfaces.Services;
using ComodatoModel = Projeto.Moope.Comodato.Core.Models.Comodato;

namespace Projeto.Moope.Comodato.Core.Services
{
    public class ComodatoService : BaseService, IComodatoService
    {
        private readonly IComodatoRepository _comodatoRepository;

        public ComodatoService(
            IComodatoRepository comodatoRepository,
            INotificador notificador) : base(notificador)
        {
            _comodatoRepository = comodatoRepository;
        }

        public async Task<ComodatoModel?> BuscarPorIdAsync(Guid id)
        {
            return await _comodatoRepository.BuscarPorIdAsync(id);
        }

        public async Task<ComodatoModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _comodatoRepository.BuscarPorIdAsNotrackingAsync(id);
        }

        public async Task<IEnumerable<ComodatoModel>> BuscarTodosAsync()
        {
            return await _comodatoRepository.BuscarTodosAsync();
        }

        public async Task<IEnumerable<ComodatoModel>> BuscarPorClienteIdAsync(Guid clienteId)
        {
            return await _comodatoRepository.BuscarPorClienteIdAsync(clienteId);
        }

        public async Task<Result<ComodatoModel>> SalvarAsync(ComodatoModel comodato)
        {
            var entity = await _comodatoRepository.SalvarAsync(comodato);
            return new Result<ComodatoModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<ComodatoModel>> AtualizarAsync(ComodatoModel comodato)
        {
            var entity = await _comodatoRepository.AtualizarAsync(comodato);
            return new Result<ComodatoModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<ComodatoModel>> AlterarStatusAsync(ComodatoModel comodato, ComodatoStatus status)
        {
            comodato.Status = status;
            var entity = await _comodatoRepository.AtualizarAsync(comodato);
            return new Result<ComodatoModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _comodatoRepository.RemoverAsync(id);
            return true;
        }
    }
}
