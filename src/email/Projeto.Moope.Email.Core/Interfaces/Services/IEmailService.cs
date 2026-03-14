using Projeto.Moope.Core.DTOs;
using EmailModel = Projeto.Moope.Email.Core.Models.Email;

namespace Projeto.Moope.Email.Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task<EmailModel?> BuscarPorIdAsync(Guid id);
        Task<EmailModel?> BuscarPorIdAsNoTrackingAsync(Guid id);
        Task<IEnumerable<EmailModel>> BuscarTodosAsync();
        Task<Result<EmailModel>> SalvarAsync(EmailModel email);
        Task<Result<EmailModel>> AtualizarAsync(EmailModel email);
        Task<bool> RemoverAsync(Guid id);
    }
}
