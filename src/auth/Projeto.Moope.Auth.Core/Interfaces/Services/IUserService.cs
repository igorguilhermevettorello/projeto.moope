using Projeto.Moope.Auth.Core.Events;
using System.Threading.Tasks;

namespace Projeto.Moope.Auth.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task CreateUser(string nome, string email, string password);
    }
}