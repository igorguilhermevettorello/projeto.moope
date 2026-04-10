using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Gateways.Api.DTOs;
using Projeto.Moope.Gateways.Core.Models;
using Projeto.Moope.Gateways.Core.Services;

namespace Projeto.Moope.Gateways.Api.Controllers
{
    [ApiController]
    [Route("api/bff/venda")]
    public class VendaBffController : MainController
    {
        private readonly IProcessarVendaOrchestrator _orchestrator;

        public VendaBffController(
            IProcessarVendaOrchestrator orchestrator,
            INotificador notificador)
            : base(notificador)
        {
            _orchestrator = orchestrator;
        }

        [AllowAnonymous]
        [HttpPost("processar")]
        [ProducesResponseType(typeof(ProcessarVendaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Processar(
            [FromBody] ProcessarVendaRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var usuario = LerUsuarioContexto(HttpContext);
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var idempotencyKey = Request.Headers.TryGetValue("Idempotency-Key", out var idem)
                ? idem.ToString()
                : null;

            var resultado = await _orchestrator.ExecutarAsync(
                MapearInput(request),
                usuario,
                string.IsNullOrWhiteSpace(authorizationHeader) ? null : authorizationHeader,
                idempotencyKey,
                cancellationToken);

            if (!resultado.Sucesso)
                return StatusCode(resultado.StatusCode, resultado.CorpoErro);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return Ok(new ProcessarVendaResponse
            {
                PlanoId = resultado.Dados.PlanoId,
                Quantidade = resultado.Dados.Quantidade,
                ValorTotal = resultado.Dados.ValorTotal,
                VendaId = resultado.Dados.VendaId,
                TransacaoId = resultado.Dados.TransacaoId
            });
        }

        private static ProcessarVendaUsuarioContext? LerUsuarioContexto(HttpContext httpContext)
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
                return null;

            var idStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("sub");

            Guid? usuarioId = Guid.TryParse(idStr, out var g) ? g : null;

            return new ProcessarVendaUsuarioContext
            {
                IsAdministrador = httpContext.User.IsInRole(nameof(TipoUsuario.Administrador)),
                UsuarioId = usuarioId
            };
        }

        private static ProcessarVendaInput MapearInput(ProcessarVendaRequest request)
        {
            return new ProcessarVendaInput
            {
                NomeCliente = request.NomeCliente,
                Email = request.Email,
                Telefone = request.Telefone,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                VendedorId = request.VendedorId,
                CodigoCupom = request.CodigoCupom,
                PlanoId = request.PlanoId ?? Guid.Empty,
                Quantidade = request.Quantidade,
                NomeCartao = request.NomeCartao,
                NumeroCartao = request.NumeroCartao,
                Cvv = request.Cvv,
                DataValidade = request.DataValidade,
                Estado = request.Estado,
                Descontos = request.Descontos,
                ComodatoToken = request.ComodatoToken
            };
        }
    }
}
