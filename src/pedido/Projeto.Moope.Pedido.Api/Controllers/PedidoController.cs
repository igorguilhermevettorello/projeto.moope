using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Pedido.Api.DTOs;
using Projeto.Moope.Pedido.Core.DTOs.Pedido;
using Projeto.Moope.Pedido.Core.Excecoes.Idempotencia;
using Projeto.Moope.Pedido.Core.Interfaces.Services;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Api.Controllers
{
    [ApiController]
    [Route("api/pedido")]
    [Authorize]
    public class PedidoController : MainController
    {
        private readonly IPedidoService _pedidoService;
        private readonly IIdempotenciaService _idempotenciaService;
        private readonly IGeradorHashRequisicao _geradorHashRequisicao;
        private readonly IMapper _mapper;

        public PedidoController(
            IPedidoService pedidoService,
            IIdempotenciaService idempotenciaService,
            IGeradorHashRequisicao geradorHashRequisicao,
            IMapper mapper,
            INotificador notificador,
            IUser appUser) : base(notificador, appUser)
        {
            _pedidoService = pedidoService;
            _idempotenciaService = idempotenciaService;
            _geradorHashRequisicao = geradorHashRequisicao;
            _mapper = mapper;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PedidoCreateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do pedido é obrigatório");
                return CustomResponse(ModelState);
            }

            var pedido = await _pedidoService.BuscarPorIdComDadosAsync(id);
            if (pedido == null)
                return NotFound("Pedido não encontrado");

            return Ok(MapToResponseDto(pedido));
        }

        [HttpGet("{id:guid}/valores-pagamento")]
        [ProducesResponseType(typeof(PedidoValoresPagamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObterValoresPagamento(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do pedido é obrigatório");
                return CustomResponse(ModelState);
            }

            var pedido = await _pedidoService.BuscarPorIdComDadosAsync(id);
            if (pedido == null)
                return NotFound("Pedido não encontrado");

            var quantidade = pedido.Quantidade;

            //var valorTotalTaxaAdesao = Math.Round(pedido.PlanoTaxaAdesao * quantidade, 2);
            //var mensalidadeTotal = Math.Round(pedido.PlanoValorComDesconto * quantidade, 2);

            var valorTotalTaxaAdesao = pedido.PlanoTaxaAdesaoTotal;
            var mensalidadeTotal = pedido.PlanoValorTotal;

            var response = new PedidoValoresPagamentoResponseDto
            {
                PedidoId = pedido.Id,
                ValorTotalTaxaAdesao = valorTotalTaxaAdesao,
                ValorTotalMensalidade = mensalidadeTotal
            };

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PedidoCreateResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] PedidoCreateRequestDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (!Request.Headers.TryGetValue("Idempotency-Key", out var idem))
                return BadRequest(new { mensagem = "Header Idempotency-Key e obrigatorio." });

            var idempotencyKey = idem.ToString();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return BadRequest(new { mensagem = "Header Idempotency-Key e obrigatorio." });

            var pedidoCreateDto = _mapper.Map<PedidoCreateDto>(dto);

            var scope = "Pedido:Criar";
            var requestHash = _geradorHashRequisicao.GerarHash(dto);

            try
            {
                var inicio = await _idempotenciaService.IniciarProcessamentoAsync(
                    idempotencyKey,
                    scope,
                    requestHash,
                    HttpContext.RequestAborted);

                if (inicio.JaConcluido && inicio.ResponseStatusCode.HasValue && !string.IsNullOrWhiteSpace(inicio.ResponseBody))
                {
                    return new ContentResult
                    {
                        StatusCode = inicio.ResponseStatusCode.Value,
                        ContentType = "application/json",
                        Content = inicio.ResponseBody
                    };
                }

                if (!inicio.DeveProcessar)
                    return Conflict("Uma solicitação com a mesma chave de idempotência já está em processamento.");

                var result = await _pedidoService.SalvarAsync(pedidoCreateDto);
                if (!result.Status)
                {
                    await _idempotenciaService.MarcarFalhaAsync(inicio.IdempotenciaId, HttpContext.RequestAborted);
                    return CustomResponse(result);
                }

                var responseDto = MapToResponseDto(result.Dados!);
                var responseJson = JsonSerializer.Serialize(responseDto);

                await _idempotenciaService.ConcluirAsync(
                    inicio.IdempotenciaId,
                    StatusCodes.Status201Created,
                    responseJson,
                    result.Dados!.Id.ToString(),
                    "Pedido",
                    HttpContext.RequestAborted);

                return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados!.Id }, responseDto);
            }
            catch (ChaveIdempotenteReutilizadaComPayloadDiferenteException ex)
            {
                NotificarErro("Idempotencia", ex.Message);
                return CustomResponse();
            }
        }

        [HttpPut("{id:guid}/transacoes")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarTransacoes(Guid id, [FromBody] PedidoUpdateTransacoesDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != dto.PedidoId)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var transacoes = dto.Transacoes.Select(t => new TransacaoModel
            {
                Valor = t.Valor,
                DataPagamento = t.DataPagamento,
                StatusPagamento = t.StatusPagamento,
                Status = t.Status,
                StatusDescricao = t.StatusDescricao,
                GalaxPayId = t.GalaxPayId,
                MetodoPagamento = t.MetodoPagamento
            }).ToList();

            var result = await _pedidoService.AtualizarTransacoesAsync(id, transacoes);
            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao atualizar transações do pedido");

                if (string.Equals(result.Mensagem, "Pedido não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result.Mensagem);

                return CustomResponse();
            }

            return NoContent();
        }

        [HttpPatch("{id:guid}/galaxpayid/{galaxPayId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarGalaxPayId(Guid id, int galaxPayId)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do pedido é obrigatório");
                return CustomResponse(ModelState);
            }

            if (galaxPayId <= 0)
            {
                ModelState.AddModelError("GalaxPayId", "GalaxPayId deve ser maior que zero");
                return CustomResponse(ModelState);
            }

            var result = await _pedidoService.AtualizarGalaxPayIdAsync(id, galaxPayId);
            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao atualizar GalaxPayId do pedido");

                if (string.Equals(result.Mensagem, "Pedido não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result.Mensagem);

                return CustomResponse();
            }

            return NoContent();
        }

        private static PedidoCreateResponseDto MapToResponseDto(PedidoModel pedido)
        {
            return new PedidoCreateResponseDto
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                VendedorId = pedido.VendedorId,
                PlanoId = pedido.PlanoId,
                Quantidade = pedido.Quantidade,
                TipoPessoa = pedido.TipoPessoa,
                Estado = pedido.Estado,
                PlanoValor = pedido.PlanoValor,
                PlanoDescricao = pedido.PlanoDescricao,
                PlanoCodigo = pedido.PlanoCodigo,
                PlanoTaxaAdesao = pedido.PlanoTaxaAdesao,
                PlanoPercentualDesconto = pedido.PlanoPercentualDesconto,
                PlanoValorTotal = pedido.PlanoValorTotal,
                PlanoTaxaAdesaoTotal = pedido.PlanoTaxaAdesaoTotal,
                StatusAssinatura = pedido.StatusAssinatura,
                Status = pedido.Status,
                StatusDescricao = pedido.StatusDescricao,
                GalaxPayId = pedido.GalaxPayId,
                Created = pedido.Created,
                Updated = pedido.Updated,
                Desconto = pedido.Desconto == null
                    ? null
                    : new DescontoDto
                    {
                        ValorPercentual = pedido.Desconto.ValorPercentual,
                        Descricao = pedido.Desconto.Descricao,
                        TipoPessoa = pedido.Desconto.TipoPessoa,
                        CodigoDesconto = pedido.Desconto.CodigoDesconto,
                        ValorDesconto = pedido.Desconto.ValorDesconto,
                        Ativo = pedido.Desconto.Ativo
                    },
                Transacoes = pedido.Transacoes?.Select(t => new TransacaoDto
                {
                    Valor = t.Valor,
                    DataPagamento = t.DataPagamento,
                    StatusPagamento = t.StatusPagamento,
                    Status = t.Status,
                    StatusDescricao = t.StatusDescricao,
                    GalaxPayId = t.GalaxPayId,
                    MetodoPagamento = t.MetodoPagamento
                }).ToList()
            };
        }
    }
}
