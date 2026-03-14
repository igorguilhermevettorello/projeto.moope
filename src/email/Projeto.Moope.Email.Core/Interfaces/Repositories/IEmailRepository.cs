using Projeto.Moope.Core.Interfaces.Data;
using EmailModel = Projeto.Moope.Email.Core.Models.Email;

namespace Projeto.Moope.Email.Core.Interfaces.Repositories
{
    public interface IEmailRepository : IRepository<EmailModel>
    {
        Task<EmailModel?> BuscarPorIdAsNoTrackingAsync(Guid id);
        Task<IEnumerable<EmailModel>> BuscarPendentesAsync();
        Task<IEnumerable<EmailModel>> BuscarAgendadosParaEnvioAsync();
        Task<IEnumerable<EmailModel>> BuscarFalhasParaReprocessarAsync(int maxTentativas);
    }
}
