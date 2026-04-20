using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Core.Interfaces.Services;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Core.Services
{
    public class PedidoService : BaseService, IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;

        public PedidoService(IPedidoRepository pedidoRepository, INotificador notificador) : base(notificador)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task<PedidoModel?> BuscarPorIdAsync(Guid id)
        {
            return await _pedidoRepository.BuscarPorIdAsync(id);
        }

        public async Task<PedidoModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _pedidoRepository.BuscarPorIdAsNotrackingAsync(id);
        }

        public async Task<PedidoModel?> BuscarPorIdComDadosAsync(Guid id)
        {
            return await _pedidoRepository.BuscarPorIdComTransacoesEDescontoAsync(id);
        }

        public async Task<Result<PedidoModel>> SalvarAsync(PedidoModel pedido)
        {
            var agora = DateTime.UtcNow;
            pedido.Created = agora;
            pedido.Updated = agora;

            if (pedido.Transacoes is { Count: > 0 })
            {
                foreach (var transacao in pedido.Transacoes)
                {
                    transacao.PedidoId = pedido.Id;
                    transacao.Created = agora;
                    transacao.Updated = agora;
                }
            }

            if (pedido.Desconto != null)
            {
                pedido.Desconto.PedidoId = pedido.Id;
            }

            var entity = await _pedidoRepository.SalvarAsync(pedido);

            return new Result<PedidoModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result> AtualizarTransacoesAsync(Guid pedidoId, IEnumerable<TransacaoModel> transacoes)
        {
            try
            {
                if (pedidoId == Guid.Empty)
                    return new Result { Status = false, Mensagem = "Pedido inválido" };

                var pedido = await _pedidoRepository.BuscarPorIdAsync(pedidoId);
                if (pedido == null || pedido.Id == Guid.Empty)
                    return new Result { Status = false, Mensagem = "Pedido não encontrado" };

                var lista = transacoes?.ToList() ?? new List<TransacaoModel>();
                var agora = DateTime.UtcNow;

                foreach (var transacao in lista)
                {
                    transacao.PedidoId = pedidoId;
                    transacao.Created = agora;
                    transacao.Updated = agora;
                }

                await _pedidoRepository.RemoverTransacoesPorPedidoIdAsync(pedidoId);

                if (lista.Count > 0)
                    await _pedidoRepository.SalvarTransacoesAsync(lista);

                pedido.Updated = agora;
                await _pedidoRepository.AtualizarAsync(pedido);

                _ = await _pedidoRepository.UnitOfWork.Commit();

                return new Result { Status = true };
            }
            catch (Exception)
            {
                return new Result { Status = false, Mensagem = "Erro ao atualizar transações do pedido" };
            }
        }
    }
}

