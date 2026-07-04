using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Comodato.Api.DTOs;
using Projeto.Moope.Comodato.Core.DTOs;
using Projeto.Moope.Comodato.Core.Interfaces.Services;
using Projeto.Moope.Comodato.Core.Models;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;

namespace Projeto.Moope.Comodato.Api.Controllers
{
    [ApiController]
    [Route("api/comodato/convites")]
    [Authorize]
    public class ComodatoConviteController : MainController
    {
        private readonly IComodatoConviteService _comodatoConviteService;

        public ComodatoConviteController(
            IComodatoConviteService comodatoConviteService,
            INotificador notificador,
            IUser appUser) : base(notificador, appUser)
        {
            _comodatoConviteService = comodatoConviteService;
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ComodatoConviteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            var convite = await _comodatoConviteService.BuscarPorIdAsNotrackingAsync(id);
            if (convite == null)
                return NotFound();

            return Ok(MapToResponseDto(convite));
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ComodatoConviteResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] CriarComodatoConviteDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (!UsuarioAutenticado)
                return Unauthorized();

            var input = new CriarComodatoConviteInput
            {
                Quantidade = dto.Quantidade,
                VendedorId = dto.VendedorId,
                PlanoId = dto.PlanoId,
                Valor = dto.Valor,
                Estado = dto.Estado,
                DataPagamento = dto.DataPagamento
            };

            var result = await _comodatoConviteService.CriarAsync(input, UsuarioId);
            if (!result.Status)
                return CustomResponse(result);

            return CreatedAtAction(
                nameof(BuscarPorId),
                new { id = result.Dados!.Convite.Id },
                MapToResponseDto(result.Dados.Convite, result.Dados.Token));
        }

        private static ComodatoConviteResponseDto MapToResponseDto(ComodatoConvite convite, string? token = null)
        {
            return new ComodatoConviteResponseDto
            {
                Id = convite.Id,
                Token = token,
                PlanoId = convite.PlanoId,
                Quantidade = convite.Quantidade,
                Valor = convite.Valor,
                VendedorId = convite.VendedorId,
                Estado = convite.Estado,
                DataPagamento = convite.DataPagamento,
                CriadoEm = convite.CriadoEm,
                ExpiradoEm = convite.ExpiradoEm,
                Status = convite.Status,
                CreatedByAdminId = convite.CreatedByAdminId
            };
        }
    }
}
