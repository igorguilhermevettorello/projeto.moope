using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Cliente.Api.DTOs.Clientes;
using Projeto.Moope.Cliente.Api.Utils;
using Projeto.Moope.Cliente.Core.Commands.Clientes.Atualizar;
using Projeto.Moope.Cliente.Core.Commands.Clientes.Criar;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using System.Security.Claims;

namespace Projeto.Moope.Cliente.Api.Controllers
{
    [ApiController]
    [Route("api/cliente")]
    [Authorize]
    public class ClienteController : MainController
    {
        private readonly IClienteService _clienteService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public ClienteController(
            IClienteService clienteService,
            IMapper mapper,
            IMediator mediator,
            INotificador notificador) : base(notificador)
        {
            _clienteService = clienteService;
            _mapper = mapper;
            _mediator = mediator;
        }

        private Guid UsuarioId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        private Task<bool> IsAdminAsync() =>
            Task.FromResult(User.IsInRole(nameof(TipoUsuario.Administrador)));

        [HttpGet]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(IEnumerable<ClienteQueryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodosAsync()
        {
            var clientes = await _clienteService.BuscarClientesComDadosAsync<ClienteQueryDto>();
            return Ok(clientes);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteDetalheDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do cliente é obrigatório");
                return CustomResponse(ModelState);
            }

            var cliente = await _clienteService.BuscarClientePorIdComDadosAsync<ClienteDetalheDto>(id);

            if (cliente == null)
                return NotFound("Cliente não encontrado");

            return Ok(cliente);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ListClienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "Email é obrigatório");
                return CustomResponse(ModelState);
            }

            var cliente = await _clienteService.BuscarPorEmailAsync(email);
            if (cliente == null)
                return NotFound("Cliente não encontrado");

            var detalhe = await _clienteService.BuscarClientePorIdComDadosAsync<ClienteDetalheDto>(cliente.Id);
            if (detalhe == null)
                return NotFound("Cliente não encontrado");

            var lista = new ListClienteDto
            {
                Nome = detalhe.Nome,
                Email = detalhe.Email,
                CpfCnpj = detalhe.CpfCnpj ?? string.Empty,
                Telefone = detalhe.Telefone ?? string.Empty
            };

            return Ok(lista);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(TipoUsuario.Vendedor)},{nameof(TipoUsuario.Administrador)}")]
        [ProducesResponseType(typeof(CreateClienteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CriarAsync([FromBody] CreateClienteDto createClienteDto)
        {
            if (createClienteDto == null)
            {
                NotificarErro("Mensagem", "As informações do cliente não foram carregadas. Tente novamente.");
                return CustomResponse();
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var command = _mapper.Map<CriarClienteCommand>(createClienteDto);

                if (!await IsAdminAsync())
                    command.VendedorId = UsuarioId == Guid.Empty ? null : UsuarioId;

                var resultado = await _mediator.Send(command);

                if (!resultado.Status)
                    return CustomResponse();

                return Created(string.Empty, new { id = resultado.Dados });
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(UpdateClienteDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarAsync(Guid id, [FromBody] UpdateClienteDto updateClienteDto)
        {
            if (id == Guid.Empty || updateClienteDto.Id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }

            if (id != updateClienteDto.Id)
            {
                ModelState.AddModelError("Id", "Campo Id do parâmetro não confere com o Id solicitado.");
                return CustomResponse(ModelState);
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var command = _mapper.Map<AtualizarClienteCommand>(updateClienteDto);
                var resultado = await _mediator.Send(command);

                if (!resultado.Status)
                    return CustomResponse();

                return NoContent();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }

        [HttpPut("ativar/{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtivarAsync(Guid id)
        {
            var cliente = await _clienteService.BuscarPorIdAsync(id);
            if (cliente == null)
                return NotFound("Cliente não encontrado");

            cliente.Updated = DateTime.UtcNow;

            var result = await _clienteService.AtualizarAsync(cliente);

            if (!result.Status)
                return CustomResponse();

            return NoContent();
        }

        [HttpPut("inativar/{id:guid}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> InativarAsync(Guid id)
        {
            var cliente = await _clienteService.BuscarPorIdAsync(id);
            if (cliente == null)
                return NotFound("Cliente não encontrado");

            cliente.Updated = DateTime.UtcNow;

            var result = await _clienteService.AtualizarAsync(cliente);

            if (!result.Status)
                return CustomResponse();

            return NoContent();
        }

        [HttpGet("tipo-pessoa")]
        [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
        public IActionResult BuscarTipoPessoasAsync()
        {
            var lista = EnumHelper.GetEnumAsList<TipoPessoa>();
            return Ok(lista);
        }

        [HttpPost("alterar-senha")]
        [Authorize(Roles = nameof(TipoUsuario.Cliente))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AlterarSenhaAsync([FromBody] AlterarSenhaClienteDto alterarSenhaDto)
        {
            if (alterarSenhaDto == null)
            {
                NotificarErro("Mensagem", "As informações da alteração de senha não foram carregadas. Tente novamente.");
                return CustomResponse();
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var clienteId = UsuarioId;

                if (clienteId == Guid.Empty)
                {
                    NotificarErro("Usuário", "Usuário não identificado");
                    return CustomResponse();
                }

                var resultado = await _clienteService.AlterarSenhaClienteAsync(
                    clienteId,
                    alterarSenhaDto.SenhaAtual,
                    alterarSenhaDto.NovaSenha);

                if (!resultado.Status)
                    return CustomResponse();

                return NoContent();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }

        [HttpPost("alterar-senha-admin")]
        [Authorize(Roles = $"{nameof(TipoUsuario.Administrador)},{nameof(TipoUsuario.Vendedor)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AlterarSenhaAdminAsync([FromBody] AlterarSenhaAdminDto alterarSenhaDto)
        {
            if (alterarSenhaDto == null)
            {
                NotificarErro("Mensagem", "As informações da alteração de senha não foram carregadas. Tente novamente.");
                return CustomResponse();
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var resultado = await _clienteService.AlterarSenhaAdminAsync(
                    alterarSenhaDto.ClienteId,
                    alterarSenhaDto.NovaSenha);

                if (!resultado.Status)
                    return CustomResponse();

                return NoContent();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }
    }
}
