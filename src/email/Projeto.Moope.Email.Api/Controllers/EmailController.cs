using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Email.Api.DTOs;
using Projeto.Moope.Email.Core.Interfaces.Services;
using Projeto.Moope.Email.Core.Enums;
using EmailModel = Projeto.Moope.Email.Core.Models.Email;

namespace Projeto.Moope.Email.Api.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : MainController
    {
        private readonly IEmailService _emailService;
        private readonly IEmailGateway _emailGateway;

        public EmailController(
            IEmailService emailService,
            IEmailGateway emailGateway,
            INotificador notificador) : base(notificador)
        {
            _emailService = emailService;
            _emailGateway = emailGateway;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarTodos()
        {
            var emails = await _emailService.BuscarTodosAsync();
            var result = emails.Select(MapToResponseDto);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            var email = await _emailService.BuscarPorIdAsNoTrackingAsync(id);
            if (email == null)
                return NotFound();

            return Ok(MapToResponseDto(email));
        }

        [HttpPost]
        [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarEmailDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var email = new EmailModel
            {
                Remetente = dto.Remetente,
                NomeRemetente = dto.NomeRemetente,
                Destinatario = dto.Destinatario,
                NomeDestinatario = dto.NomeDestinatario,
                Copia = dto.Copia,
                CopiaOculta = dto.CopiaOculta,
                Assunto = dto.Assunto,
                Corpo = dto.Corpo,
                EhHtml = dto.EhHtml,
                Prioridade = dto.Prioridade,
                Status = StatusEmail.Pendente,
                DataProgramada = dto.DataProgramada,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            var result = await _emailService.SalvarAsync(email);
            if (!result.Status)
                return CustomResponse(result);

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados!.Id }, MapToResponseDto(result.Dados!));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(EmailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AlterarEmailDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (id != dto.Id)
                return BadRequest("O Id da URL deve corresponder ao Id do corpo.");

            var existing = await _emailService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.Remetente = dto.Remetente;
            existing.NomeRemetente = dto.NomeRemetente;
            existing.Destinatario = dto.Destinatario;
            existing.NomeDestinatario = dto.NomeDestinatario;
            existing.Copia = dto.Copia;
            existing.CopiaOculta = dto.CopiaOculta;
            existing.Assunto = dto.Assunto;
            existing.Corpo = dto.Corpo;
            existing.EhHtml = dto.EhHtml;
            existing.Prioridade = dto.Prioridade;
            existing.Status = dto.Status;
            existing.DataProgramada = dto.DataProgramada;
            existing.Updated = DateTime.UtcNow;

            var result = await _emailService.AtualizarAsync(existing);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(MapToResponseDto(result.Dados!));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover(Guid id)
        {
            var existing = await _emailService.BuscarPorIdAsync(id);
            if (existing == null)
                return NotFound();

            await _emailService.RemoverAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Envia email via requisição HTTP ao endpoint configurado (IEmailGateway).
        /// </summary>
        [HttpPost("enviar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Enviar([FromBody] EnviarEmailDto dto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var request = new Projeto.Moope.Email.Core.Interfaces.Services.EnvioEmailRequest(
                Destinatario: dto.Destinatario,
                Assunto: dto.Assunto,
                Corpo: dto.Corpo,
                Remetente: dto.Remetente,
                NomeRemetente: dto.NomeRemetente,
                Copia: dto.Copia,
                CopiaOculta: dto.CopiaOculta,
                EhHtml: dto.EhHtml
            );

            var resultado = await _emailGateway.EnviarAsync(request);

            if (!resultado.Sucesso)
                return CustomResponse();

            return Ok(new
            {
                sucesso = true,
                mensagem = resultado.MensagemSucesso ?? "Email enviado com sucesso"
            });
        }

        private static EmailResponseDto MapToResponseDto(EmailModel email)
        {
            return new EmailResponseDto
            {
                Id = email.Id,
                Remetente = email.Remetente,
                NomeRemetente = email.NomeRemetente,
                Destinatario = email.Destinatario,
                NomeDestinatario = email.NomeDestinatario,
                Copia = email.Copia,
                CopiaOculta = email.CopiaOculta,
                Assunto = email.Assunto,
                Corpo = email.Corpo,
                EhHtml = email.EhHtml,
                Prioridade = email.Prioridade,
                Status = email.Status,
                TentativasEnvio = email.TentativasEnvio,
                UltimaTentativa = email.UltimaTentativa,
                DataEnvio = email.DataEnvio,
                MensagemErro = email.MensagemErro,
                MensagemSucesso = email.MensagemSucesso,
                DataProgramada = email.DataProgramada,
                Created = email.Created,
                Updated = email.Updated
            };
        }
    }
}
