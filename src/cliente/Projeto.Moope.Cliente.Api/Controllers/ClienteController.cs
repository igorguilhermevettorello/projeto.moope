using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Cliente.Api.DTOs;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Gateways.Api.Utils;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Api.Controllers
{
    [ApiController]
    [Route("api/cliente")]
    [Authorize]
    public class ClienteController : MainController
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService, INotificador notificador, IUser appUser) : base(notificador, appUser)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodos()
        {
            var clientes = await _clienteService.BuscarClientesComDadosAsync<ClienteQueryDto>();
            return Ok(clientes);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do cliente é obrigatório");
                return CustomResponse(ModelState);
            }

            var cliente = await _clienteService.BuscarClientePorIdComDadosAsync<ClienteDetalheDto>(id);

            if (cliente == null)
            {
                return NotFound("Cliente não encontrado");
            }

            return Ok(cliente);
        }

        //[HttpGet("cupom/{codigoCupom}")]
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> BuscarPorCodigoCupom(string codigoCupom)
        //{
        //    var cliente = await _clienteService.BuscarPorCodigoCupomAsync(codigoCupom);
        //    if (cliente == null)
        //        return NotFound();

        //    return Ok(MapToResponseDto(cliente));
        //}

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] CriarClienteDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var cliente = new ClienteModel
            {
                TipoPessoa = dto.TipoPessoa,
                CpfCnpj = dto.CpfCnpj,
                VendedorId = dto.VendedorId
            };

            if (dto.UsuarioId != Guid.Empty && dto.UsuarioId != null)
                cliente.Id = (Guid)dto.UsuarioId;

            var result = await _clienteService.SalvarAsync(cliente);
            if (!result.Status)
                return CustomResponse(result);

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados!.Id }, MapToResponseDto(result.Dados!));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AlterarClienteDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != dto.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var existing = await _clienteService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.TipoPessoa = dto.TipoPessoa;
            existing.CpfCnpj = dto.CpfCnpj;
            existing.VendedorId = dto.VendedorId;

            var result = await _clienteService.AtualizarAsync(existing);
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
            var existing = await _clienteService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            await _clienteService.RemoverAsync(id);
            return NoContent();
        }

        [HttpPatch("{clienteId}/endereco/{enderecoId}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarEndereco(Guid clienteId, Guid enderecoId)
        {
            if (clienteId == Guid.Empty || enderecoId == Guid.Empty)
            {
                ModelState.AddModelError("Id", "Usuário e o Endereço são obrigatórios.");
                return CustomResponse(ModelState);
            }

            var result = await _clienteService.AtualizarEndereco(clienteId, enderecoId);

            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao atualizar endereço do usuário");

                if (string.Equals(result.Mensagem, "Usuário não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result.Mensagem);

                return CustomResponse();
            }

            return NoContent();
        }

        [HttpGet("tipo-pessoa")]
        [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult BuscarTipoPessoasAsync()
        {
            var lista = EnumHelper.GetEnumAsList<TipoPessoa>();
            return Ok(lista);
        }

        private static ClienteResponseDto MapToResponseDto(ClienteModel cliente)
        {
            return new ClienteResponseDto
            {
                Id = cliente.Id,
                TipoPessoa = cliente.TipoPessoa,
                CpfCnpj = cliente.CpfCnpj,
                Created = cliente.Created,
                Updated = cliente.Updated,
                VendedorId = cliente.VendedorId
            };
        }
    }
}
