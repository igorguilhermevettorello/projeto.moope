using MediatR;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Core.Interfaces.Services;
using Projeto.Moope.Pedido.Core.Queries.Plano.ObterPlanoPorId;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Core.Services
{
    public class PedidoService : BaseService, IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMediator _mediator;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            IMediator mediator,
            INotificador notificador) : base(notificador)
        {
            _pedidoRepository = pedidoRepository;
            _mediator = mediator;
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

        public async Task<ResultDto<PedidoModel>> SalvarAsync(PedidoModel pedido)
        {
            if (pedido.PlanoId == Guid.Empty)
            {
                return new ResultDto<PedidoModel>
                {
                    Status = false,
                    Mensagem = "Identificador do plano é obrigatório"
                };
            }

            var plano = await _mediator.Send(new ObterPlanoPorIdQuery { PlanoId = pedido.PlanoId });
            if (plano == null)
            {
                return new ResultDto<PedidoModel>
                {
                    Status = false,
                    Mensagem = "Plano não encontrado ou indisponível na API de planos"
                };
            }

            if (!plano.Status)
            {
                return new ResultDto<PedidoModel>
                {
                    Status = false,
                    Mensagem = "Plano inativo"
                };
            }

            pedido.PlanoCodigo = plano.Codigo;
            pedido.PlanoDescricao = plano.Descricao;
            pedido.PlanoValor = plano.Valor;
            pedido.PlanoTaxaAdesao = plano.TaxaAdesao ?? 0;

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
                pedido.Desconto.PedidoId = pedido.Id;

            var entity = await _pedidoRepository.SalvarAsync(pedido);

            return new ResultDto<PedidoModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<ResultDto<PedidoModel>> AtualizarTransacoesAsync(Guid pedidoId, IEnumerable<TransacaoModel> transacoes)
        {
            try
            {
                if (pedidoId == Guid.Empty)
                {
                    return new ResultDto<PedidoModel>
                    {
                        Status = false,
                        Mensagem = "Pedido inválido"
                    };
                }

                var pedido = await _pedidoRepository.BuscarPorIdAsync(pedidoId);
                if (pedido == null || pedido.Id == Guid.Empty)
                {
                    return new ResultDto<PedidoModel>
                    {
                        Status = false,
                        Mensagem = "Pedido não encontrado"
                    };
                }

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

                return new ResultDto<PedidoModel> { Status = true, Dados = pedido };
            }
            catch (Exception)
            {
                return new ResultDto<PedidoModel>
                {
                    Status = false,
                    Mensagem = "Erro ao atualizar transações do pedido"
                };
            }
        }
    }
}
