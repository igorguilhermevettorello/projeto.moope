using Projeto.Moope.Auth.Core.Models;

namespace Projeto.Moope.Auth.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task Add(Usuario usuario);
        Task<Usuario?> GetByEmail(string email);
    }
}