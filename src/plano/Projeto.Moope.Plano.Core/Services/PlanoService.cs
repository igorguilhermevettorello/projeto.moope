using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Plano.Core.Interfaces.Repositories;
using Projeto.Moope.Plano.Core.Interfaces.Services;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Core.Services
{
    public class PlanoService : BaseService, IPlanoService
    {
        private readonly IPlanoRepository _planoRepository;
        public PlanoService(
            IPlanoRepository planoRepository,
            INotificador notificador) : base(notificador)
        {
            _planoRepository = planoRepository;
        }

        public async Task<PlanoModel?> BuscarPorIdAsync(Guid id)
        {
            return await _planoRepository.BuscarPorIdAsync(id);
        }

        public async Task<PlanoModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _planoRepository.BuscarPorIdAsNotrackingAsync(id);
        }

        public async Task<PlanoModel?> BuscarPorPlanoSelecionadoAsync(string codigo)
        {
            return await _planoRepository.BuscarPorPlanoSelecionadoAsync(codigo);
        }


        public async Task<IEnumerable<PlanoModel>> BuscarTodosAsync()
        {
            return await _planoRepository.BuscarTodosAsync();
        }

        public async Task<Result<PlanoModel>> SalvarAsync(PlanoModel plano)
        {
            //if (!ExecutarValidacao(new PlanoValidator(), plano))
            //{
            //    return new Result<PlanoModel>()
            //    {
            //        Status = false,
            //        Mensagem = "Não foi possível salvar o plano. Por favor, tente novamente em alguns instantes."
            //    };
            //}

            var entity = await _planoRepository.SalvarAsync(plano);
            return new Result<PlanoModel>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<PlanoModel>> AtualizarAsync(PlanoModel plano)
        {
            //if (!ExecutarValidacao(new PlanoValidator(), plano))
            //{
            //    return new Result<PlanoModel>()
            //    {
            //        Status = false
            //    };
            //}

            var entity = await _planoRepository.AtualizarAsync(plano);
            return new Result<PlanoModel>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _planoRepository.RemoverAsync(id);
            return true;
        }

        public async Task<Result<PlanoModel>> AtivarInativarAsync(PlanoModel plano, bool status)
        {
            plano.Status = status;
            var entity = await _planoRepository.AtualizarAsync(plano);
            return new Result<PlanoModel>()
            {
                Status = true,
                Dados = entity
            };
        }
    }
}
