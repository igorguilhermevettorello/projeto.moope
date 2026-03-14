using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Comodato.Api.DTOs;
using Projeto.Moope.Comodato.Core.Interfaces.Services;
using ComodatoModel = Projeto.Moope.Comodato.Core.Models.Comodato;

namespace Projeto.Moope.Comodato.Api.Controllers
{
    [ApiController]
    [Route("api/comodato")]
    public class ComodatoController : MainController
    {
        private readonly IComodatoService _comodatoService;

        public ComodatoController(IComodatoService comodatoService, INotificador notificador) : base(notificador)
        {
            _comodatoService = comodatoService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ComodatoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarTodos()
        {
            var comodatos = await _comodatoService.BuscarTodosAsync();
            var result = comodatos.Select(MapToResponseDto);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ComodatoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            var comodato = await _comodatoService.BuscarPorIdAsNotrackingAsync(id);
            if (comodato == null)
                return NotFound();

            return Ok(MapToResponseDto(comodato));
        }

        [HttpGet("cliente/{clienteId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<ComodatoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarPorClienteId(Guid clienteId)
        {
            var comodatos = await _comodatoService.BuscarPorClienteIdAsync(clienteId);
            var result = comodatos.Select(MapToResponseDto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ComodatoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarComodatoDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var comodato = new ComodatoModel
            {
                ClienteId = dto.ClienteId,
                ProdutoNome = dto.ProdutoNome,
                Valor = dto.Valor,
                CriadoEm = DateTime.UtcNow,
                Status = dto.Status
            };

            var result = await _comodatoService.SalvarAsync(comodato);
            if (!result.Status)
                return CustomResponse(result);

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados!.Id }, MapToResponseDto(result.Dados!));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ComodatoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AlterarComodatoDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != dto.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var existing = await _comodatoService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.ClienteId = dto.ClienteId;
            existing.ProdutoNome = dto.ProdutoNome;
            existing.Valor = dto.Valor;
            existing.Status = dto.Status;

            var result = await _comodatoService.AtualizarAsync(existing);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(MapToResponseDto(result.Dados!));
        }

        [HttpPatch("{id:guid}/alterar-status")]
        [ProducesResponseType(typeof(ComodatoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AlterarStatus(Guid id, [FromQuery] ComodatoStatus status)
        {
            var existing = await _comodatoService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            var result = await _comodatoService.AlterarStatusAsync(existing, status);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(MapToResponseDto(result.Dados!));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover(Guid id)
        {
            var existing = await _comodatoService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            await _comodatoService.RemoverAsync(id);
            return NoContent();
        }

        private static ComodatoResponseDto MapToResponseDto(ComodatoModel comodato)
        {
            return new ComodatoResponseDto
            {
                Id = comodato.Id,
                ClienteId = comodato.ClienteId,
                ProdutoNome = comodato.ProdutoNome,
                Valor = comodato.Valor,
                CriadoEm = comodato.CriadoEm,
                Status = comodato.Status
            };
        }
    }
}
