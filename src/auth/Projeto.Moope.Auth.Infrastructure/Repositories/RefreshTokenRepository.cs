using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Auth.Infrastructure.Data;
using Projeto.Moope.Core.Interfaces.Data;

namespace Projeto.Moope.Auth.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppAuthContext _context;

        public RefreshTokenRepository(AppAuthContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<RefreshToken?> BuscarPorTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task<RefreshToken> SalvarAsync(RefreshToken entity)
        {
            await _context.RefreshTokens.AddAsync(entity);
            return entity;
        }

        public async Task<RefreshToken> AtualizarAsync(RefreshToken entity)
        {
            _context.RefreshTokens.Update(entity);
            return entity;
        }

        public async Task RevogarTokensPorUsuarioAsync(Guid usuarioId)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UsuarioId == usuarioId && r.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }
    }
}
