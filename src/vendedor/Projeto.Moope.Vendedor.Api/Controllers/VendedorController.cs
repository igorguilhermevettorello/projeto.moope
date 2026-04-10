using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Vendedor.Api.DTOs;
using Projeto.Moope.Vendedor.Core.Interfaces.Services;
using VendedorModel = Projeto.Moope.Vendedor.Core.Models.Vendedor;

namespace Projeto.Moope.Vendedor.Api.Controllers
{
    [ApiController]
    [Route("api/vendedor")]
    [Authorize]
    public class VendedorController : MainController
    {
        private readonly IVendedorService _vendedorService;

        public VendedorController(IVendedorService vendedorService, INotificador notificador) : base(notificador)
        {
            _vendedorService = vendedorService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VendedorResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodos()
        {
            var vendedores = await _vendedorService.BuscarVendedoresComDadosAsync<VendedorQueryDto>();
            return Ok(vendedores);

            //var vendedores = await _vendedorService.BuscarTodosAsync();
            //var result = vendedores.Select(MapToResponseDto);
            //return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(VendedorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do vendedor é obrigatório");
                return CustomResponse(ModelState);
            }

            var vendedor = await _vendedorService.BuscarVendedorPorIdComDadosAsync<VendedorDetalheDto>(id);

            if (vendedor == null)
            {
                return NotFound("Vendedor não encontrado");
            }

            return Ok(vendedor);

            //var vendedor = await _vendedorService.BuscarPorIdAsNotrackingAsync(id);
            //if (vendedor == null)
            //    return NotFound();

            //return Ok(MapToResponseDto(vendedor));
        }

        [HttpGet("cupom/{codigoCupom}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(VendedorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarPorCodigoCupom(string codigoCupom)
        {
            var vendedor = await _vendedorService.BuscarPorCodigoCupomAsync(codigoCupom);
            if (vendedor == null)
                return NotFound();

            return Ok(MapToResponseDto(vendedor));
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(VendedorResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] CriarVendedorDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var vendedor = new VendedorModel
            {
                TipoPessoa = dto.TipoPessoa,
                CpfCnpj = dto.CpfCnpj,
                PercentualComissao = dto.PercentualComissao,
                ChavePix = dto.ChavePix,
                CodigoCupom = dto.CodigoCupom,
                VendedorId = dto.VendedorId
            };

            if (dto.UsuarioId != Guid.Empty && dto.UsuarioId != null)
                vendedor.Id = (Guid) dto.UsuarioId;

            var result = await _vendedorService.SalvarAsync(vendedor);
            if (!result.Status)
                return CustomResponse(result);

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados!.Id }, MapToResponseDto(result.Dados!));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(VendedorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AlterarVendedorDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != dto.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var existing = await _vendedorService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.TipoPessoa = dto.TipoPessoa;
            existing.CpfCnpj = dto.CpfCnpj;
            existing.PercentualComissao = dto.PercentualComissao;
            existing.ChavePix = dto.ChavePix;
            existing.CodigoCupom = dto.CodigoCupom;
            existing.VendedorId = dto.VendedorId;

            var result = await _vendedorService.AtualizarAsync(existing);
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
            var existing = await _vendedorService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            await _vendedorService.RemoverAsync(id);
            return NoContent();
        }

        [HttpGet("tipo-pessoa")]
        [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult BuscarTipoPessoasAsync()
        {
            // Retorna apenas PessoaJuridica
            var lista = new List<object>
            {
                new { value = (int)TipoPessoa.JURIDICA, label = "Pessoa Jurídica" }
            };

            return Ok(lista);
        }

        private static VendedorResponseDto MapToResponseDto(VendedorModel vendedor)
        {
            return new VendedorResponseDto
            {
                Id = vendedor.Id,
                TipoPessoa = vendedor.TipoPessoa,
                CpfCnpj = vendedor.CpfCnpj,
                PercentualComissao = vendedor.PercentualComissao,
                ChavePix = vendedor.ChavePix,
                CodigoCupom = vendedor.CodigoCupom,
                Created = vendedor.Created,
                Updated = vendedor.Updated,
                VendedorId = vendedor.VendedorId
            };
        }
    }
}
