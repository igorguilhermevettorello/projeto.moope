using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Auth.Application.Commands.Usuario.Alterar;
using Projeto.Moope.Auth.Application.Commands.Usuario.AtualizarEndereco;
using Projeto.Moope.Auth.Application.Commands.Usuario.AlterarSenha;
using Projeto.Moope.Auth.Application.Commands.Usuario.Criar;
using Projeto.Moope.Auth.Application.Commands.Usuario.Deletar;
using Projeto.Moope.Auth.Application.Queries.Usuario.Listar;
using Projeto.Moope.Auth.Application.Queries.Usuario.ObterPorId;
using Projeto.Moope.Auth.Core.DTOs.Usuario;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;

namespace Projeto.Moope.Auth.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController : MainController
    {
        private readonly IMediator _mediator;

        public UsuarioController(INotificador notificador, IMediator mediator) : base(notificador)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(IEnumerable<ListarUsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Listar()
        {
            var result = await _mediator.Send(new ListarUsuarioQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(UsuarioDetalheDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do usuário é obrigatório");
                return CustomResponse(ModelState);
            }

            var result = await _mediator.Send(new ObterUsuarioPorIdQuery { Id = id });

            if (result == null)
            {
                return NotFound("Usuário não encontrado");
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CriarUsuarioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CriarUsuarioCommand
            {
                Nome = dto.Nome,
                Email = dto.Email,
                CpfCnpj = dto.CpfCnpj,
                Telefone = dto.Telefone,
                TipoPessoa = dto.TipoPessoa,
                TipoUsuario = dto.TipoUsuario,
                Senha = dto.Senha,
                Confirmacao = dto.Confirmacao,
                NomeFantasia = dto.NomeFantasia ?? string.Empty,
                InscricaoEstadual = dto.InscricaoEstadual ?? string.Empty
            };

            var result = await _mediator.Send(command);

            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao criar usuário");
                return CustomResponse();
            }

            return CreatedAtAction(nameof(ObterPorId), new { id = result.Dados }, result.Dados);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Alterar(Guid id, [FromBody] AlterarUsuarioDto dto)
        {
            if (id == Guid.Empty || dto.Id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }

            if (id != dto.Id)
            {
                ModelState.AddModelError("Id", "Campo Id do parâmetro não confere com o Id solicitado.");
                return CustomResponse(ModelState);
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var command = new AlterarUsuarioCommand
            {
                Id = dto.Id,
                Nome = dto.Nome,
                Email = dto.Email,
                CpfCnpj = dto.CpfCnpj,
                Telefone = dto.Telefone,
                TipoPessoa = dto.TipoPessoa,
                NomeFantasia = dto.NomeFantasia ?? string.Empty,
                InscricaoEstadual = dto.InscricaoEstadual ?? string.Empty,
                VendedorId = dto.VendedorId
            };

            var result = await _mediator.Send(command);

            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao alterar usuário");
                return CustomResponse();
            }

            return NoContent();
        }

        [HttpPatch("{usuarioId}/endereco/{enderecoId}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarEndereco(Guid usuarioId, Guid enderecoId)
        {
            if (usuarioId == Guid.Empty || enderecoId == Guid.Empty)
            {
                ModelState.AddModelError("Id", "UsuarioId e EnderecoId são obrigatórios.");
                return CustomResponse(ModelState);
            }

            var command = new AtualizarEnderecoUsuarioCommand
            {
                UsuarioId = usuarioId,
                EnderecoId = enderecoId
            };

            var result = await _mediator.Send(command);

            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao atualizar endereço do usuário");

                if (string.Equals(result.Mensagem, "Usuário não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result.Mensagem);

                return CustomResponse();
            }

            return NoContent();
        }

        [HttpPost("alterar-senha")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaUsuarioDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var command = new AlterarSenhaUsuarioCommand
            {
                UsuarioId = dto.UsuarioId,
                SenhaAtual = dto.SenhaAtual,
                NovaSenha = dto.NovaSenha,
                ConfirmacaoNovaSenha = dto.ConfirmacaoNovaSenha
            };

            var result = await _mediator.Send(command);

            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao alterar senha");
                return CustomResponse();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Deletar(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do usuário é obrigatório");
                return CustomResponse(ModelState);
            }

            var command = new DeletarUsuarioCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.Status)
            {
                NotificarErro("Mensagem", result.Mensagem ?? "Erro ao deletar usuário");
                return CustomResponse();
            }

            return NoContent();
        }
    }
}
