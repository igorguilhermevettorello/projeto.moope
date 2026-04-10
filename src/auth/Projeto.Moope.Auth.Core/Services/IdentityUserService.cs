using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Auth.Core.Services
{
    public class IdentityUserService : IIdentityUserService
    {
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        
        public IdentityUserService(UserManager<IdentityUser<Guid>> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ResultUser<IdentityUser<Guid>>> CriarUsuarioAsync(
            string email,
            string senha,
            string telefone = null,
            TipoUsuario tipoUsuario = TipoUsuario.Cliente)
        {
            var identityUser = new IdentityUser<Guid>
            {
                UserName = email,
                Email = email,
                PhoneNumber = telefone
            };

            var result = await _userManager.CreateAsync(identityUser, senha);

            if (result.Succeeded)
            {
                // Optionally add to role based on TipoUsuario
                await _userManager.AddToRoleAsync(identityUser, tipoUsuario.ToString());

                return new ResultUser<IdentityUser<Guid>> { Status = true, Dados = identityUser };
            }
            else
            {
                return new ResultUser<IdentityUser<Guid>>
                {
                    Status = false,
                    Mensagem = string.Join("; ", result.Errors.Select(e => e.Description))
                };
            }
        }

        public async Task RemoverAoFalharAsync(IdentityUser<Guid> usuario)
        {
            await _userManager.DeleteAsync(usuario);
        }

        public async Task<Result<IdentityUser<Guid>>> AlterarUsuarioAsync(Guid userId, string email, string telefone = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new Result<IdentityUser<Guid>> { Status = false, Mensagem = "User not found" };
            }

            user.Email = email;
            user.UserName = email;
            user.PhoneNumber = telefone;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new Result<IdentityUser<Guid>> { Status = true, Dados = user };
            }
            else
            {
                return new Result<IdentityUser<Guid>>
                {
                    Status = false,
                    Mensagem = string.Join("; ", result.Errors.Select(e => e.Description))
                };
            }
        }

        public async Task<IdentityUser<Guid>> BuscarPorEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityUser<Guid>?> BuscarPorIdAsync(Guid id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        public async Task<Result> AlterarSenhaAsync(Guid userId, string senhaAtual, string novaSenha)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new Result { Status = false, Mensagem = "Usuário não encontrado" };
            }

            var result = await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);
            if (result.Succeeded)
            {
                return new Result { Status = true };
            }

            return new Result
            {
                Status = false,
                Mensagem = string.Join("; ", result.Errors.Select(e => e.Description))
            };
        }

        public async Task<Result> RemoverUsuarioAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new Result { Status = false, Mensagem = "Usuário não encontrado" };
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return new Result { Status = true };
            }

            return new Result
            {
                Status = false,
                Mensagem = string.Join("; ", result.Errors.Select(e => e.Description))
            };
        }
    }
}
