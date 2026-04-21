using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Gateways.Api.DTOs.Vendedor;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor;

namespace Projeto.Moope.Gateways.Api.Controllers
{
    [ApiController]
    [Route("api/bff/vendedor")]
    [Authorize]
    public class VendedorBffController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IVendedorCreateService _vendedorCreateService;
        private readonly IVendedorGetByIdService _vendedorGetByIdService;
        private readonly IVendedorUpdateService _vendedorUpdateService;

        public VendedorBffController(
            IMapper mapper,
            IVendedorCreateService vendedorCreateService,
            IVendedorGetByIdService vendedorGetByIdService,
            IVendedorUpdateService vendedorUpdateService,
            INotificador notificador,
            IUser appUser)
            : base(notificador, appUser)
        {
            _mapper = mapper;
            _vendedorCreateService = vendedorCreateService;
            _vendedorGetByIdService = vendedorGetByIdService;
            _vendedorUpdateService = vendedorUpdateService;
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(VendedorCreateResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Cadastrar([FromBody] VendedorCreateRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _vendedorCreateService.ExecutarAsync(
                _mapper.Map<VendedorCreateDto>(request),
                authorizationHeader,
                cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return StatusCode(StatusCodes.Status201Created, new VendedorCreateResponseDto
            {
                VendedorId = resultado.Dados.VendedorId,
                EnderecoId = resultado.Dados.EnderecoId
            });
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(VendedorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _vendedorGetByIdService.ExecutarAsync(id, authorizationHeader, cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return Ok(resultado.Dados);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(VendedorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] VendedorUpdateRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != request.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _vendedorUpdateService.ExecutarAsync(
                _mapper.Map<VendedorUpdateDto>(request),
                authorizationHeader,
                cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            if (resultado.Dados == null)
                return StatusCode(StatusCodes.Status502BadGateway, new { mensagem = "Resposta invalida da orquestracao." });

            return Ok(resultado.Dados);
        }

    }
}
