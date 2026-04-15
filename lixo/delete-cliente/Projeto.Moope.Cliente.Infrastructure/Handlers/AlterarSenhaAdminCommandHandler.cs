using MediatR;
using Projeto.Moope.Cliente.Core.Commands.Clientes.AlterarSenha;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Cliente.Infrastructure.Handlers
{
    public class AlterarSenhaAdminCommandHandler : IRequestHandler<AlterarSenhaAdminCommand, Result>
    {
        private readonly IIdentityUserService _identityUserService;
        private readonly INotificador _notificador;

        public AlterarSenhaAdminCommandHandler(
            IIdentityUserService identityUserService,
            INotificador notificador)
        {
            _identityUserService = identityUserService;
            _notificador = notificador;
        }

        public Task<Result> Handle(AlterarSenhaAdminCommand request, CancellationToken cancellationToken)
        {
            _notificador.Handle(new Notificacao
            {
                Campo = "Mensagem",
                Mensagem = "Alteração administrativa de senha foi centralizada no BC Auth e deve ser executada por endpoint próprio do Auth."
            });

            return Task.FromResult(new Result
            {
                Status = false,
                Mensagem = "Operação indisponível no BC Cliente. Utilize o BC Auth para alteração administrativa de senha."
            });
        }
    }
}
