using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Plano.Api.DTOs;
using Projeto.Moope.Plano.Core.Interfaces.Services;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Api.Controllers
{
    [ApiController]
    [Route("api/plano")]
    [Authorize]
    public class PlanoController : MainController
    {
        private readonly IPlanoService _planoService;

        public PlanoController(IPlanoService planoService, INotificador notificador) : base(notificador)
        {
            _planoService = planoService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PlanoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodos()
        {
            var planos = await _planoService.BuscarTodosAsync();
            var result = planos.Select(MapToResponseDto);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PlanoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            var plano = await _planoService.BuscarPorIdAsNotrackingAsync(id);
            if (plano == null)
                return NotFound();

            return Ok(MapToResponseDto(plano));
        }

        [HttpGet("codigo/{codigo}")]
        [ProducesResponseType(typeof(PlanoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarPorCodigo(string codigo)
        {
            var plano = await _planoService.BuscarPorPlanoSelecionadoAsync(codigo);
            if (plano == null)
                return NotFound();

            return Ok(MapToResponseDto(plano));
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(PlanoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] CriarPlanoDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var plano = new PlanoModel
            {
                Codigo = dto.Codigo,
                Descricao = dto.Descricao,
                Valor = dto.Valor,
                TaxaAdesao = dto.TaxaAdesao,
                Status = dto.Status,
                Plataforma = dto.Plataforma
            };

            var result = await _planoService.SalvarAsync(plano);
            if (!result.Status)
                return CustomResponse(result);

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados!.Id }, MapToResponseDto(result.Dados!));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(PlanoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AlterarPlanoDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != dto.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var existing = await _planoService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.Codigo = dto.Codigo;
            existing.Descricao = dto.Descricao;
            existing.Valor = dto.Valor;
            existing.TaxaAdesao = dto.TaxaAdesao;
            existing.Status = dto.Status;
            existing.Plataforma = dto.Plataforma;

            var result = await _planoService.AtualizarAsync(existing);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(MapToResponseDto(result.Dados!));
        }

        [HttpPatch("{id:guid}/ativar-inativar")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(PlanoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtivarInativar(Guid id, [FromQuery] bool status)
        {
            var existing = await _planoService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            var result = await _planoService.AtivarInativarAsync(existing, status);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(MapToResponseDto(result.Dados!));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Remover(Guid id)
        {
            var existing = await _planoService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            await _planoService.RemoverAsync(id);
            return NoContent();
        }

        private static PlanoResponseDto MapToResponseDto(PlanoModel plano)
        {
            return new PlanoResponseDto
            {
                Id = plano.Id,
                Codigo = plano.Codigo,
                Descricao = plano.Descricao,
                Valor = plano.Valor,
                TaxaAdesao = plano.TaxaAdesao,
                Status = plano.Status,
                Plataforma = plano.Plataforma
            };
        }
    }
}
