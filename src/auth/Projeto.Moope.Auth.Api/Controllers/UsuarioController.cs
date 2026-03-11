using MediatR;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Auth.Application.Commands.Usuario.Criar;
using Projeto.Moope.Auth.Core.DTOs.Usuario;
using Projeto.Moope.Core.Interfaces.Notificacao;

namespace Projeto.Moope.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController : MainController
    {
        private readonly IMediator _mediator;

        public UsuarioController(INotificador notificador, IMediator mediator) : base(notificador)
        {
            _mediator = mediator;
        }

        [HttpPost]
        //[Authorize(Roles = nameof(TipoUsuario.Administrador))]
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
                Senha = dto.Senha,
                Confirmacao = dto.Confirmacao,
                NomeFantasia = dto.NomeFantasia,
                InscricaoEstadual = dto.InscricaoEstadual,
                VendedorId = dto.VendedorId
            };

            var result = await _mediator.Send(command);

            //if (!result.Status)
            //{
            //    return BadRequest(result.Mensagem);
            //}

            //return CreatedAtAction(nameof(Create), new { id = result.Dados }, result.Dados);
            return Ok();
        }
    }
}

