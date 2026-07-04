using System.Security.Cryptography;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Api.Utils;
using Projeto.Moope.Cliente.Api.Configurations;
using Projeto.Moope.Cliente.Api.DTOs;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Api.Controllers
{
    [ApiController]
    [Route("api/cliente")]
    [Authorize]
    public class ClienteController : MainController
    {
        private readonly IClienteService _clienteService;
        private readonly AnonymousEndpointKeysSettings _anonymousEndpointKeysSettings;

        public ClienteController(
            IClienteService clienteService,
            IOptions<AnonymousEndpointKeysSettings> anonymousEndpointKeysSettings,
            INotificador notificador,
            IUser appUser) : base(notificador, appUser)
        {
            _clienteService = clienteService;
            _anonymousEndpointKeysSettings = anonymousEndpointKeysSettings.Value;
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

        [HttpGet("vendedor")]
        [Authorize(Roles = nameof(TipoUsuario.Vendedor))]
        [ProducesResponseType(typeof(IEnumerable<ClienteQueryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarPorVendedorLogado()
        {
            if (UsuarioId == Guid.Empty)
                return Unauthorized();

            var clientes = await _clienteService.BuscarClientesPorVendedorComDadosAsync<ClienteQueryDto>(UsuarioId);
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

            var cliente = await _clienteService.BuscarClientePorIdComDadosAsync<ClienteDetailResponseDto>(id);

            if (cliente == null)
            {
                return NotFound("Cliente não encontrado");
            }

            return Ok(cliente);
        }

        [HttpGet("email")]
        [AllowAnonymous]
        [ApiKeyRequired]
        [ProducesResponseType(typeof(ClienteDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorEmail([FromQuery] string email)
        {
            if (!TryValidateApiKey())
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email é obrigatório.");

            if (!IsValidEmail(email))
                return BadRequest("Email inválido.");

            var cliente = await _clienteService.BuscarClientePorEmailComDadosAsync<ClienteDetailResponseDto>(email);
            if (cliente == null)
                return NotFound("Cliente não encontrado");

            return Ok(cliente);
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteCreateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var cliente = new ClienteModel
            {
                Telefone = dto.Telefone,
                TelefoneEmergencia = dto.TelefoneEmergencia,
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
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != dto.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var existing = await _clienteService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.Telefone = dto.Telefone;
            existing.TelefoneEmergencia = dto.TelefoneEmergencia;
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

        [HttpPatch("{clienteId}/galaxpay/{galaxPayId}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarGalaxPayId(Guid clienteId, int galaxPayId)
        {
            if (clienteId == Guid.Empty || galaxPayId == 0)
            {
                ModelState.AddModelError("Id", "Usuário e o GalaxPayId são obrigatórios.");
                return CustomResponse(ModelState);
            }

            var result = await _clienteService.AtualizarGalaxPayId(clienteId, galaxPayId);
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

        private bool TryValidateApiKey()
        {
            if (!Request.Headers.TryGetValue("x-api-key", out var apiKeyHeaderValues))
                return false;

            var providedApiKey = apiKeyHeaderValues.ToString();
            if (string.IsNullOrWhiteSpace(providedApiKey))
                return false;

            var configuredApiKey = _anonymousEndpointKeysSettings.BuscarClientePorEmail;
            if (string.IsNullOrWhiteSpace(configuredApiKey))
                return false;

            return FixedTimeEquals(providedApiKey.Trim(), configuredApiKey.Trim());
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = System.Text.Encoding.UTF8.GetBytes(left);
            var rightBytes = System.Text.Encoding.UTF8.GetBytes(right);
            if (leftBytes.Length != rightBytes.Length)
                return false;
            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
