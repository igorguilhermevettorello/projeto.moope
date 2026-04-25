using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Pagamento.Api.Security;
using Projeto.Moope.Pagamento.Core.DTOs.Intencao;
using Projeto.Moope.Pagamento.Core.Interfaces.Services;

namespace Projeto.Moope.Pagamento.Api.Controllers
{
    [ApiController]
    [Route("api/pagamentos")]
    public class IntencoesPagamentoController : MainController
    {
        private readonly IIntencaoPagamentoService _intencaoPagamentoService;

        public IntencoesPagamentoController(
            IIntencaoPagamentoService intencaoPagamentoService,
            INotificador notificador,
            IUser appUser)
            : base(notificador, appUser)
        {
            _intencaoPagamentoService = intencaoPagamentoService;
        }

        [HttpPost("intencoes")]
        [RequireApiKey]
        [ProducesResponseType(typeof(CriarIntencaoPagamentoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar(
            [FromBody] CriarIntencaoPagamentoRequestDto requisicao,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var resultado = await _intencaoPagamentoService.CriarAsync(requisicao, cancellationToken);
            if (resultado is null)
                return CustomResponse();

            return CreatedAtAction(
                nameof(Obter),
                new { id = resultado.IdempotencyKey },
                resultado);
        }

        [HttpGet("intencoes/{id:guid}")]
        [RequireApiKey]
        [ProducesResponseType(typeof(CriarIntencaoPagamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Obter(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var resultado = await _intencaoPagamentoService.ObterPorIdAsync(id, cancellationToken);
            if (resultado is null)
                return NotFound();

            return Ok(resultado);
        }
    }
}
