using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Gateways.Api.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.Interfaces.Services;

namespace Projeto.Moope.Gateways.Api.Controllers
{
    [ApiController]
    [Route("api/bff/cliente")]
    [Authorize]
    public class ClienteBffController : MainController
    {
        private readonly IClienteCreateService _clienteCreateService;

        public ClienteBffController(
            IClienteCreateService clienteCreateService,
            INotificador notificador,
            IUser appUser)
            : base(notificador, appUser)
        {
            _clienteCreateService = clienteCreateService;
        }

        [HttpPost("cadastro")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteCreateResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> CadastrarComposto([FromBody] ClienteCreateRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _clienteCreateService.ExecutarAsync(
                MapearParaInput(request),
                authorizationHeader,
                cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return StatusCode(StatusCodes.Status201Created, new ClienteCreateResponseDto
            {
                ClienteId = resultado.Dados.ClienteId,
                EnderecoId = resultado.Dados.EnderecoId
            });
        }

        private static ClienteCreateDto MapearParaInput(ClienteCreateRequestDto request)
        {
            return new ClienteCreateDto
            {
                Nome = request.Nome,
                Email = request.Email,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                Telefone = request.Telefone,
                Ativo = request.Ativo,
                Endereco = new EnderecoCreateDto
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
