using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Gateways.Api.DTOs.Venda;
using Projeto.Moope.Gateways.Core.DTOs.Venda;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using System.Security.Claims;

namespace Projeto.Moope.Gateways.Api.Controllers
{
    [ApiController]
    [Route("api/bff/venda")]
    public class VendaBffController : MainController
    {
        private readonly IProcessarVendaService _processarVendaService;
        private readonly IMapper _mapper;

        public VendaBffController(
            IProcessarVendaService processarVendaService,
            IMapper mapper,
            INotificador notificador,
            IUser appUser)
            : base(notificador, appUser)
        {
            _processarVendaService = processarVendaService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("processar")]
        [ProducesResponseType(typeof(VendaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Processar([FromBody] VendaRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var usuario = LerUsuarioContexto(HttpContext);
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var idempotencyKey = Request.Headers.TryGetValue("Idempotency-Key", out var idem)
                ? idem.ToString()
                : null;

            var resultado = await _processarVendaService.ExecutarAsync(
                _mapper.Map<VendaCreateDto>(request),
                usuario,
                string.IsNullOrWhiteSpace(authorizationHeader) ? null : authorizationHeader,
                idempotencyKey,
                cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            //return Ok(new VendaResponseDto
            //{
            //    PlanoId = resultado.Dados.PlanoId,
            //    Quantidade = resultado.Dados.Quantidade,
            //    ValorTotal = resultado.Dados.ValorTotal,
            //    VendaId = resultado.Dados.VendaId,
            //    TransacaoId = resultado.Dados.TransacaoId
            //});
            return Ok();
        }

        private static VendaUsuarioDto? LerUsuarioContexto(HttpContext httpContext)
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
                return null;

            var idStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("sub");

            Guid? usuarioId = Guid.TryParse(idStr, out var g) ? g : null;

            return new VendaUsuarioDto
            {
                IsAdministrador = httpContext.User.IsInRole(nameof(TipoUsuario.Administrador)),
                UsuarioId = usuarioId
            };
        }

    }
}
