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
    [Route("api/bff/cliente")]
    [Authorize]
    public class ClienteBffController : MainController
    {
        private readonly ICadastroClienteOrchestrator _orchestrator;

        public ClienteBffController(
            ICadastroClienteOrchestrator orchestrator,
            INotificador notificador)
            : base(notificador)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("cadastro")]
        [Authorize(Roles = $"{nameof(TipoUsuario.Vendedor)},{nameof(TipoUsuario.Administrador)}")]
        [ProducesResponseType(typeof(CadastrarClienteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Cadastrar(
            [FromBody] CadastrarClienteRequest request,
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

            return StatusCode(StatusCodes.Status201Created, new CadastrarClienteResponse
            {
                ClienteId = resultado.Dados.ClienteId
            });
        }

        private static CadastrarClienteInput MapearParaInput(CadastrarClienteRequest request)
        {
            return new CadastrarClienteInput
            {
                Nome = request.Nome,
                Email = request.Email,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                Telefone = request.Telefone,
                Ativo = request.Ativo,
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
                Senha = request.Senha,
                Confirmacao = request.Confirmacao,
                NomeFantasia = request.NomeFantasia,
                InscricaoEstadual = request.InscricaoEstadual,
                VendedorId = request.VendedorId
            };
        }
    }
}
