using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Endereco.Api.DTOs;
using Projeto.Moope.Endereco.Core.Interfaces.Services;
using EnderecoModel = Projeto.Moope.Endereco.Core.Models.Endereco;

namespace Projeto.Moope.Endereco.Api.Controllers
{
    [ApiController]
    [Route("api/endereco")]
    public class EnderecoController : MainController
    {
        private readonly IEnderecoService _enderecoService;


        public EnderecoController(IEnderecoService enderecoService, INotificador notificador) : base(notificador)
        {
            _enderecoService = enderecoService;
        }

        [HttpGet]
        //[Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ListarEnderecoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var enderecos = await _enderecoService.GetAll();
            var result = enderecos.Select(e => new ListarEnderecoDto
            {
                Id = e.Id,
                Cep = e.Cep,
                Logradouro = e.Logradouro,
                Numero = e.Numero,
                Complemento = e.Complemento,
                Bairro = e.Bairro,
                Cidade = e.Cidade,
                Estado = e.Estado
            });
            return Ok(result);
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ListarEnderecoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var endereco = await _enderecoService.GetById(id);
            if (endereco == null)
                return NotFound();

            var result = new ListarEnderecoDto
            {
                Id = endereco.Id,
                Cep = endereco.Cep,
                Logradouro = endereco.Logradouro,
                Numero = endereco.Numero,
                Complemento = endereco.Complemento,
                Bairro = endereco.Bairro,
                Cidade = endereco.Cidade,
                Estado = endereco.Estado
            };
            return Ok(result);
        }

        [HttpPost]
        //[Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ListarEnderecoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CriarEnderecoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var endereco = new EnderecoModel
            {
                Cep = dto.Cep,
                Logradouro = dto.Logradouro,
                Numero = dto.Numero,
                Complemento = dto.Complemento,
                Bairro = dto.Bairro,
                Cidade = dto.Cidade,
                Estado = dto.Estado
            };

            await _enderecoService.Create(endereco);
            return CreatedAtAction(nameof(GetById), new { id = endereco.Id }, endereco.Id);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AlterarEnderecoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Id == Guid.Empty)
                return BadRequest("Id é obrigatório.");

            if (id != dto.Id)
                return BadRequest("Id da URL deve corresponder ao Id do corpo.");

            if (dto.Id != Guid.Empty)
            {
                var exists = await _enderecoService.GetById(dto.Id);
                if (exists == null)
                    return NotFound();
            }

            var existing = await _enderecoService.GetById(dto.Id);
            if (existing == null)
                return NotFound();

            existing.Cep = dto.Cep;
            existing.Logradouro = dto.Logradouro;
            existing.Numero = dto.Numero;
            existing.Complemento = dto.Complemento;
            existing.Bairro = dto.Bairro;
            existing.Cidade = dto.Cidade;
            existing.Estado = dto.Estado;

            await _enderecoService.Update(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _enderecoService.GetById(id);
            if (existing == null)
                return NotFound();

            await _enderecoService.Delete(id);
            return NoContent();
        }
    }
}