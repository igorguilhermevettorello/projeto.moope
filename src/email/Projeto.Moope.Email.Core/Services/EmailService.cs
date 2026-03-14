using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Email.Core.Interfaces.Repositories;
using Projeto.Moope.Email.Core.Interfaces.Services;
using EmailModel = Projeto.Moope.Email.Core.Models.Email;

namespace Projeto.Moope.Email.Core.Services
{
    public class EmailService : BaseService, IEmailService
    {
        private readonly IEmailRepository _emailRepository;

        public EmailService(
            IEmailRepository emailRepository,
            INotificador notificador) : base(notificador)
        {
            _emailRepository = emailRepository;
        }

        public async Task<EmailModel?> BuscarPorIdAsync(Guid id)
        {
            return await _emailRepository.BuscarPorIdAsync(id);
        }

        public async Task<EmailModel?> BuscarPorIdAsNoTrackingAsync(Guid id)
        {
            return await _emailRepository.BuscarPorIdAsNoTrackingAsync(id);
        }

        public async Task<IEnumerable<EmailModel>> BuscarTodosAsync()
        {
            return await _emailRepository.BuscarTodosAsync();
        }

        public async Task<Result<EmailModel>> SalvarAsync(EmailModel email)
        {
            var entity = await _emailRepository.SalvarAsync(email);
            return new Result<EmailModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<EmailModel>> AtualizarAsync(EmailModel email)
        {
            var entity = await _emailRepository.AtualizarAsync(email);
            return new Result<EmailModel>
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _emailRepository.RemoverAsync(id);
            return true;
        }
    }
}
