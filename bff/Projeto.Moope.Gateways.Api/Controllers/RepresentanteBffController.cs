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
    [Route("api/bff/representante")]
    [Authorize]
    public class RepresentanteBffController : MainController
    {
        private readonly ICadastroRepresentanteOrchestrator _orchestrator;

        public RepresentanteBffController(
            ICadastroRepresentanteOrchestrator orchestrator,
            INotificador notificador)
            : base(notificador)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("cadastro")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(CadastroRepresentanteCompostoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> CadastrarComposto(
            [FromBody] CadastrarRepresentanteRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _orchestrator.ExecutarAsync(
                MapearParaInput(request),
                authorizationHeader,
                cancellationToken);

            if (!resultado.Sucesso)
                return StatusCode(resultado.StatusCode, resultado.CorpoErro);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return StatusCode(StatusCodes.Status201Created, new CadastroRepresentanteCompostoResponse
            {
                VendedorId = resultado.Dados.VendedorId,
                UsuarioId = resultado.Dados.UsuarioId,
                EnderecoId = resultado.Dados.EnderecoId
            });
        }

        private static CadastrarRepresentanteInput MapearParaInput(CadastrarRepresentanteRequest request)
        {
            return new CadastrarRepresentanteInput
            {
                Nome = request.Nome,
                Email = request.Email,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                Telefone = request.Telefone,
                Endereco = new RepresentanteEnderecoInput
                {
                    Cep = request.Endereco.Cep,
                    Logradouro = request.Endereco.Logradouro,
                    Numero = request.Endereco.Numero,
                    Complemento = request.Endereco.Complemento,
                    Bairro = request.Endereco.Bairro,
                    Cidade = request.Endereco.Cidade,
                    Estado = request.Endereco.Estado
                },
                NomeFantasia = request.NomeFantasia,
                InscricaoEstadual = request.InscricaoEstadual,
                PercentualComissao = request.PercentualComissao,
                ChavePix = request.ChavePix,
                Senha = request.Senha,
                Confirmacao = request.Confirmacao,
                CodigoCupom = request.CodigoCupom
            };
        }
    }
}
