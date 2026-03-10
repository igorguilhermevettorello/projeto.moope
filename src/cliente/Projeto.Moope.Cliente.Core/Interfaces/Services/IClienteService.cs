using Projeto.Moope.Cliente.Core.Models;
using System.Threading.Tasks;

namespace Projeto.Moope.Cliente.Core.Interfaces.Services
{
    public interface IClienteService
    {
        Task CreateCliente(Guid usuarioId, string nome, string email);
    }
}