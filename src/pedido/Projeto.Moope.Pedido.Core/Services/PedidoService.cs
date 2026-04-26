using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Pedido.Core.DTOs.Pedido;
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
        private readonly IDescontoService _descontoService;
        private readonly ILogger<PedidoService> _logger;
        private readonly IConfiguration _configuration;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            IMediator mediator,
            IDescontoService descontoService,
            ILogger<PedidoService> logger,
            IConfiguration configuration,
            INotificador notificador) : base(notificador)
        {
            _pedidoRepository = pedidoRepository;
            _mediator = mediator;
            _descontoService = descontoService;
            _logger = logger;
            _configuration = configuration;
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

        public async Task<ResultDto<PedidoModel>> SalvarAsync(PedidoCreateDto pedidoCreateDto)
        {
            if (pedidoCreateDto.PlanoId == Guid.Empty)
            {
                return new ResultDto<PedidoModel>
                {
                    Status = false,
                    Mensagem = "Identificador do plano é obrigatório"
                };
            }

            var plano = await _mediator.Send(new ObterPlanoPorIdQuery { PlanoId = pedidoCreateDto.PlanoId });
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

            var semTaxaAdesao = _configuration
                    .GetSection("semTaxaAdesao")
                    .GetChildren()
                    .Select(c => c.Value)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToArray();

            _logger.LogInformation("Configuração semTaxaAdesao carregada: {SemTaxaAdesao}", string.Join(", ", semTaxaAdesao));

            var codigosDesconto = new List<string>();

            decimal planoTaxaAdesao = 0;
            decimal percentualTotalDescontos = 0;
            decimal planoValorTotal = 0; // com desconto aplicado aplicado quantidade
            decimal planoTaxaAdesaoTotal = 0;
            if (plano.Plataforma)
            {
                if (pedidoCreateDto.Quantidade > 40)
                {
                    planoTaxaAdesaoTotal = 790;
                    planoValorTotal = 790;
                }
                else
                {
                    planoTaxaAdesaoTotal = Math.Round((plano.Valor * pedidoCreateDto.Quantidade), 2);
                    planoValorTotal = Math.Round((plano.Valor * pedidoCreateDto.Quantidade), 2);
                }

                planoValorTotal = plano.Valor;
                percentualTotalDescontos = 0;
                //ValorTotalTaxaAdesao = 0;
            }
            else
            {
                var estadoIsentoTaxa = !string.IsNullOrWhiteSpace(pedidoCreateDto.Estado) && semTaxaAdesao.Contains(pedidoCreateDto.Estado);
                planoTaxaAdesao = estadoIsentoTaxa ? 0 : (plano.TaxaAdesao ?? 0);
                percentualTotalDescontos = _descontoService.ObterPercentualTotalDescontosAsync(codigosDesconto, pedidoCreateDto.TipoPessoa);
                planoValorTotal = Math.Round(Math.Round(plano.Valor - ((plano.Valor * percentualTotalDescontos) / 100), 2) * pedidoCreateDto.Quantidade, 2);
                planoTaxaAdesaoTotal = estadoIsentoTaxa ? 0 : Math.Round((planoTaxaAdesao * pedidoCreateDto.Quantidade), 2);

                //PlanoValorTotal = Math.Round((planoValorComDesconto * pedidoCreateDto.Quantidade), 2);
                //ValorTotalTaxaAdesao = Math.Round((planoTaxaAdesao * pedidoCreateDto.Quantidade), 2);
            }

            var pedido = new PedidoModel
            {
                ClienteId = pedidoCreateDto.ClienteId,
                VendedorId = pedidoCreateDto.VendedorId,
                PlanoId = pedidoCreateDto.PlanoId,
                Quantidade = pedidoCreateDto.Quantidade,
                TipoPessoa = pedidoCreateDto.TipoPessoa,
                Estado = pedidoCreateDto.Estado,
                StatusAssinatura = Enums.StatusAssinatura.WaitingPayment,
                PlanoCodigo = plano.Codigo,
                PlanoDescricao = plano.Descricao,
                PlanoValor = plano.Valor,
                PlanoTaxaAdesao = plano.TaxaAdesao ?? 0,
                PlanoPercentualDesconto = percentualTotalDescontos,
                PlanoValorTotal = planoValorTotal,
                PlanoTaxaAdesaoTotal = planoTaxaAdesaoTotal,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
            };

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

        public async Task<ResultDto> AtualizarGalaxPayIdAsync(Guid pedidoId, int galaxPayId)
        {
            try
            {
                if (pedidoId == Guid.Empty)
                {
                    return new ResultDto
                    {
                        Status = false,
                        Mensagem = "Pedido inválido"
                    };
                }

                if (galaxPayId <= 0)
                {
                    return new ResultDto
                    {
                        Status = false,
                        Mensagem = "GalaxPayId inválido"
                    };
                }

                var pedido = await _pedidoRepository.BuscarPorIdAsync(pedidoId);
                if (pedido == null || pedido.Id == Guid.Empty)
                {
                    return new ResultDto
                    {
                        Status = false,
                        Mensagem = "Pedido não encontrado"
                    };
                }

                pedido.GalaxPayId = galaxPayId;
                pedido.Updated = DateTime.UtcNow;

                await _pedidoRepository.AtualizarAsync(pedido);
                _ = await _pedidoRepository.UnitOfWork.Commit();

                return new ResultDto { Status = true };
            }
            catch (Exception)
            {
                return new ResultDto
                {
                    Status = false,
                    Mensagem = "Erro ao atualizar GalaxPayId do pedido"
                };
            }
        }
    }
}
