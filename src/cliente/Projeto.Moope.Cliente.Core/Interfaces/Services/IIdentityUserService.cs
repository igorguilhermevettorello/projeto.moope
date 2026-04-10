using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Cliente.Core.Interfaces.Services
{
    public interface IIdentityUserService
    {
        Task<ResultUser<Guid>> CriarUsuarioAsync(
            string email,
            string senha,
            string? telefone = null,
            TipoUsuario tipoUsuario = TipoUsuario.Cliente);

        Task<Result> RemoverAoFalharAsync(Guid usuarioId);

        Task<Result<Guid>> AlterarUsuarioAsync(Guid userId, string email, string? telefone = null);

        Task<Guid?> BuscarPorEmailAsync(string email);

        Task<Result> AlterarSenhaAsync(Guid usuarioId, string senhaAtual, string novaSenha);
    }
}
