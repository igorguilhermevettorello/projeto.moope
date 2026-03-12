using MediatR;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Auth.Core.Interfaces.Services;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.AlterarSenha
{
    public class AlterarSenhaUsuarioCommandHandler : IRequestHandler<AlterarSenhaUsuarioCommand, Result>
    {
        private readonly IIdentityUserService _identityUserService;

        public AlterarSenhaUsuarioCommandHandler(IIdentityUserService identityUserService)
        {
            _identityUserService = identityUserService;
        }

        public async Task<Result> Handle(AlterarSenhaUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                return new Result { Status = false, Mensagem = "Dados inválidos" };
            }

            return await _identityUserService.AlterarSenhaAsync(
                request.UsuarioId,
                request.SenhaAtual,
                request.NovaSenha);
        }
    }
}
