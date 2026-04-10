using MediatR;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.AtualizarEndereco
{
    public class AtualizarEnderecoUsuarioCommandHandler : IRequestHandler<AtualizarEnderecoUsuarioCommand, Result>
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public AtualizarEnderecoUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Result> Handle(AtualizarEnderecoUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                return new Result { Status = false, Mensagem = "Dados inválidos" };
            }

            var usuario = await _usuarioRepository.BuscarPorIdAsync(request.UsuarioId);
            if (usuario == null || usuario.Id == Guid.Empty)
            {
                return new Result { Status = false, Mensagem = "Usuário não encontrado" };
            }

            usuario.EnderecoId = request.EnderecoId;
            usuario.Updated = DateTime.UtcNow;

            await _usuarioRepository.AtualizarAsync(usuario);

            var commitSucesso = await _usuarioRepository.UnitOfWork.Commit();
            if (!commitSucesso)
            {
                return new Result { Status = false, Mensagem = "Erro ao atualizar endereço do usuário" };
            }

            return new Result { Status = true };
        }
    }
}
