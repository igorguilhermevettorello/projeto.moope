using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Pedido.Api.DTOs;
using Projeto.Moope.Pedido.Core.Interfaces.Services;
using PedidoModel = Projeto.Moope.Pedido.Core.Models.Pedido;
using DescontoModel = Projeto.Moope.Pedido.Core.Models.Desconto;
using TransacaoModel = Projeto.Moope.Pedido.Core.Models.Transacao;

namespace Projeto.Moope.Pedido.Api.Controllers
{
    [ApiController]
    [Route("api/pedido")]
    [Authorize]
    public class PedidoController : MainController
    {
        private readonly IPedidoService _pedidoService;

        public PedidoController(IPedidoService pedidoService, INotificador notificador, IUser appUser) : base(notificador, appUser)
        {
            _pedidoService = pedidoService;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
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

        [HttpPost]
        [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] PedidoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var pedido = new PedidoModel
            {
                ClienteId = dto.ClienteId,
                VendedorId = dto.VendedorId,
                PlanoId = dto.PlanoId,
                Quantidade = dto.Quantidade,
                PlanoValor = dto.PlanoValor,
                PlanoDescricao = dto.PlanoDescricao,
                PlanoCodigo = dto.PlanoCodigo,
                PlanoTaxaAdesao = dto.PlanoTaxaAdesao,
                PlanoPercentualDesconto = dto.PlanoPercentualDesconto,
                PlanoValorComDesconto = dto.PlanoValorComDesconto,
                Total = dto.Total,
                StatusAssinatura = dto.StatusAssinatura,
                Status = dto.Status,
                StatusDescricao = dto.StatusDescricao,
                GalaxPayId = dto.GalaxPayId
            };

            if (dto.Desconto != null)
            {
                pedido.Desconto = new DescontoModel
                {
                    ValorPercentual = dto.Desconto.ValorPercentual,
                    Descricao = dto.Desconto.Descricao,
                    TipoPessoa = dto.Desconto.TipoPessoa,
                    CodigoDesconto = dto.Desconto.CodigoDesconto,
                    ValorDesconto = dto.Desconto.ValorDesconto,
                    Ativo = dto.Desconto.Ativo
                };
            }

            if (dto.Transacoes is { Count: > 0 })
            {
                pedido.Transacoes = dto.Transacoes.Select(t => new TransacaoModel
                {
                    Valor = t.Valor,
                    DataPagamento = t.DataPagamento,
                    StatusPagamento = t.StatusPagamento,
                    Status = t.Status,
                    StatusDescricao = t.StatusDescricao,
                    GalaxPayId = t.GalaxPayId,
                    MetodoPagamento = t.MetodoPagamento
                }).ToList();
            }

            var result = await _pedidoService.SalvarAsync(pedido);
            if (!result.Status)
                return CustomResponse(result);

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados!.Id }, MapToResponseDto(result.Dados!));
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

        private static PedidoResponseDto MapToResponseDto(PedidoModel pedido)
        {
            return new PedidoResponseDto
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                VendedorId = pedido.VendedorId,
                PlanoId = pedido.PlanoId,
                Quantidade = pedido.Quantidade,
                PlanoValor = pedido.PlanoValor,
                PlanoDescricao = pedido.PlanoDescricao,
                PlanoCodigo = pedido.PlanoCodigo,
                PlanoTaxaAdesao = pedido.PlanoTaxaAdesao,
                PlanoPercentualDesconto = pedido.PlanoPercentualDesconto,
                PlanoValorComDesconto = pedido.PlanoValorComDesconto,
                Total = pedido.Total,
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

