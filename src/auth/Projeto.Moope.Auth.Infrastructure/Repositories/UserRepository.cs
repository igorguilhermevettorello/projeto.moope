using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        // Assuming there's a DbContext, but for simplicity, mock or use existing.
        // Need to add DbSet<Usuario> to some context.

        // For now, placeholder
        public Task Add(Usuario usuario)
        {
            // Implement EF add
            throw new NotImplementedException();
        }

        public Task<Usuario?> GetByEmail(string email)
        {
            // Implement EF query
            throw new NotImplementedException();
        }
    }
}