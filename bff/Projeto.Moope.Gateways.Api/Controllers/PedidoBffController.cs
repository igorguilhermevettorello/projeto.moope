using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Gateways.Core.DTOs.Pedido;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Pedido;

namespace Projeto.Moope.Gateways.Api.Controllers
{
    [ApiController]
    [Route("api/bff/pedido")]
    [Authorize]
    public class PedidoBffController : MainController
    {
        private readonly IPedidoListService _pedidoListService;

        public PedidoBffController(
            IPedidoListService pedidoListService,
            INotificador notificador,
            IUser appUser)
            : base(notificador, appUser)
        {
            _pedidoListService = pedidoListService;
        }

        [HttpGet]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(IEnumerable<PedidoListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var resultado = await _pedidoListService.ExecutarAsync(authorizationHeader, cancellationToken);

            if (!resultado.Status)
                return StatusCode(resultado.StatusCode, resultado.Mensagem);

            return Ok(resultado.Dados ?? Array.Empty<PedidoListItemDto>());
        }
    }
}
