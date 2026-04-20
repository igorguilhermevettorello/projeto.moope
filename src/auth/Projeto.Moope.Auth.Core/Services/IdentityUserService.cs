using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;

namespace Projeto.Moope.Auth.Core.Services
{
    public class IdentityUserService : IIdentityUserService
    {
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly INotificador _notificador;

        public IdentityUserService(UserManager<IdentityUser<Guid>> userManager, INotificador notificador)
        {
            _userManager = userManager;
            _notificador = notificador;
        }

        public async Task<ResultUser<IdentityUser<Guid>>> CriarUsuarioAsync(
            string email,
            string senha,
            string telefone = null,
            TipoUsuario tipoUsuario = TipoUsuario.Cliente)
        {
            try
            {
                var usuario = new IdentityUser<Guid>
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = telefone,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };

                var usuarioExiste = await _userManager.FindByEmailAsync(email);
                if (usuarioExiste != null)
                {
                    var rs = await _userManager.GetRolesAsync(usuarioExiste);
                    if (tipoUsuario == TipoUsuario.Vendedor && rs.Contains(TipoUsuario.Vendedor.ToString()))
                    {                        
                        return new ResultUser<IdentityUser<Guid>>()
                        {
                            Status = false,
                            Mensagem = $"O usuário '{email}' já está em uso."
                        };
                    }

                    if (tipoUsuario == TipoUsuario.Cliente && rs.Contains(TipoUsuario.Cliente.ToString()))
                    {
                        return new ResultUser<IdentityUser<Guid>>()
                        {
                            Status = false,
                            Mensagem = $"O usuário '{email}' já está em uso."
                        };
                    }

                    return new ResultUser<IdentityUser<Guid>>()
                    {
                        Status = true,
                        Dados = usuarioExiste,
                        UsuarioExiste = true
                    };
                }

                var resultado = await _userManager.CreateAsync(usuario, senha);

                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(usuario, tipoUsuario.ToString());
                }
                else
                {
                    string mensagemErro = string.Join("; ", resultado.Errors.Select(e => e.Description));
                    //foreach (var error in resultado.Errors)
                    //{
                    //    if (error.Code.Equals("PasswordRequiresUpper"))
                    //    {
                    //        Notificar("Senha", error.Description);
                    //    }
                    //    else if (error.Code.Equals("PasswordRequiresLower"))
                    //    {
                    //        Notificar("Senha", error.Description);
                    //    }
                    //    else if (error.Code.Equals("PasswordRequiresDigit"))
                    //    {
                    //        Notificar("Senha", error.Description);
                    //    }
                    //    else if (error.Code.Equals("PasswordRequiresNonAlphanumeric"))
                    //    {
                    //        Notificar("Senha", error.Description);
                    //    }
                    //    else if (error.Code.Equals("DuplicateUserName"))
                    //    {
                    //        Notificar("Email", error.Description);
                    //    }
                    //    else
                    //    {
                    //        Notificar("Senha", error.Description);
                    //    }
                    //}

                    return new ResultUser<IdentityUser<Guid>>()
                    {
                        Status = false,
                        Mensagem = mensagemErro
                    };
                }

                return new ResultUser<IdentityUser<Guid>>()
                {
                    Status = true,
                    Dados = usuario
                };
            }
            catch (Exception ex)
            {
                return new ResultUser<IdentityUser<Guid>>()
                {
                    Status = false,
                    Mensagem = $"Falha ao criar usuário: {ex.Message}"
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
