using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Auth.Core.Interfaces.Services
{
    public interface IIdentityUserService
    {
        Task<ResultUser<IdentityUser<Guid>>> CriarUsuarioAsync(
            string email,
            string senha,
            string telefone = null,
            TipoUsuario tipoUsuario = TipoUsuario.Cliente);

        Task RemoverAoFalharAsync(IdentityUser<Guid> usuario);
        Task<Result<IdentityUser<Guid>>> AlterarUsuarioAsync(Guid userId, string email, string telefone = null);
        Task<Result> AlterarSenhaAsync(Guid userId, string senhaAtual, string novaSenha);
        Task<Result> RemoverUsuarioAsync(Guid userId);

        Task<IdentityUser<Guid>> BuscarPorEmailAsync(string email);
        Task<IdentityUser<Guid>?> BuscarPorIdAsync(Guid id);
    }
}
