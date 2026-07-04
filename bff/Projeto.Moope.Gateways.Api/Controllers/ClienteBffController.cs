using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Gateways.Api.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente;

namespace Projeto.Moope.Gateways.Api.Controllers
{
    [ApiController]
    [Route("api/bff/cliente")]
    [Authorize]
    public class ClienteBffController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IClienteCreateService _clienteCreateService;
        private readonly IClienteGetByIdService _clienteGetByIdService;
        private readonly IClienteUpdateService _clienteUpdateService;
        private readonly IClienteDeleteService _clienteDeleteService;
        private readonly IClienteListByVendedorService _clienteListByVendedorService;

        public ClienteBffController(
            IMapper mapper,
            IClienteCreateService clienteCreateService,
            IClienteGetByIdService clienteGetByIdService,
            IClienteUpdateService clienteUpdateService,
            IClienteDeleteService clienteDeleteService,
            IClienteListByVendedorService clienteListByVendedorService,
            INotificador notificador,
            IUser appUser)
            : base(notificador, appUser)
        {
            _mapper = mapper;
            _clienteCreateService = clienteCreateService;
            _clienteGetByIdService = clienteGetByIdService;
            _clienteUpdateService = clienteUpdateService;
            _clienteDeleteService = clienteDeleteService;
            _clienteListByVendedorService = clienteListByVendedorService;
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteCreateResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Cadastrar([FromBody] ClienteCreateRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _clienteCreateService.ExecutarAsync(
                _mapper.Map<ClienteCreateDto>(request),
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

        [HttpGet("vendedor")]
        [Authorize(Roles = nameof(TipoUsuario.Vendedor))]
        [ProducesResponseType(typeof(IEnumerable<ClienteListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> ListarPorVendedorLogado(CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _clienteListByVendedorService.ExecutarAsync(authorizationHeader, cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            return Ok(resultado.Dados ?? Array.Empty<ClienteListItemDto>());
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _clienteGetByIdService.ExecutarAsync(id, authorizationHeader, cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return Ok(resultado.Dados);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] ClienteUpdateRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != request.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _clienteUpdateService.ExecutarAsync(
                _mapper.Map<ClienteUpdateDto>(request),
                authorizationHeader,
                cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return Ok(resultado.Dados);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _clienteDeleteService.ExecutarAsync(id, authorizationHeader, cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            return NoContent();
        }
    }
}
