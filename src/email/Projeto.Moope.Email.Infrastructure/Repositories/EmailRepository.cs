using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Email.Core.Enums;
using Projeto.Moope.Email.Core.Interfaces.Repositories;
using Projeto.Moope.Email.Infrastructure.Data;
using EmailModel = Projeto.Moope.Email.Core.Models.Email;

namespace Projeto.Moope.Email.Infrastructure.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly AppEmailContext _context;

        public EmailRepository(AppEmailContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<EmailModel> BuscarPorIdAsync(Guid id)
        {
            return (await _context.Emails.FindAsync(id))!;
        }

        public async Task<EmailModel?> BuscarPorIdAsNoTrackingAsync(Guid id)
        {
            return await _context.Emails.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EmailModel>> BuscarPendentesAsync()
        {
            return await _context.Emails
                .AsNoTracking()
                .Where(e => e.Status == StatusEmail.Pendente)
                .OrderBy(e => e.Prioridade)
                .ThenBy(e => e.Created)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailModel>> BuscarAgendadosParaEnvioAsync()
        {
            var agora = DateTime.UtcNow;
            return await _context.Emails
                .AsNoTracking()
                .Where(e => e.Status == StatusEmail.Agendado && e.DataProgramada.HasValue && e.DataProgramada <= agora)
                .OrderBy(e => e.DataProgramada)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailModel>> BuscarFalhasParaReprocessarAsync(int maxTentativas)
        {
            return await _context.Emails
                .AsNoTracking()
                .Where(e => e.Status == StatusEmail.Falha && e.TentativasEnvio < maxTentativas)
                .OrderBy(e => e.UltimaTentativa)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailModel>> BuscarTodosAsync()
        {
            return await _context.Emails.ToListAsync();
        }

        public async Task<EmailModel> SalvarAsync(EmailModel entity)
        {
            await _context.Emails.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<EmailModel> AtualizarAsync(EmailModel entity)
        {
            _context.Emails.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var entity = await _context.Emails.FindAsync(id);
            if (entity == null)
                return false;

            _context.Emails.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
