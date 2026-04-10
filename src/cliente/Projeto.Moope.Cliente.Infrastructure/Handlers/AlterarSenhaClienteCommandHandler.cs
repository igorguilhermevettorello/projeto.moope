using MediatR;
using Projeto.Moope.Cliente.Core.Commands.Clientes.AlterarSenha;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Cliente.Infrastructure.Handlers
{
    public class AlterarSenhaClienteCommandHandler : IRequestHandler<AlterarSenhaClienteCommand, Result>
    {
        private readonly IIdentityUserService _identityUserService;
        private readonly INotificador _notificador;

        public AlterarSenhaClienteCommandHandler(
            IIdentityUserService identityUserService,
            INotificador notificador)
        {
            _identityUserService = identityUserService;
            _notificador = notificador;
        }

        public async Task<Result> Handle(AlterarSenhaClienteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var resultado = await _identityUserService.AlterarSenhaAsync(request.ClienteId, request.SenhaAtual, request.NovaSenha);
                if (resultado.Status)
                    return resultado;

                _notificador.Handle(new Notificacao
                {
                    Campo = "Mensagem",
                    Mensagem = resultado.Mensagem ?? "Falha ao alterar senha"
                });

                return resultado;
            }
            catch (Exception ex)
            {
                _notificador.Handle(new Notificacao
                {
                    Campo = "Mensagem",
                    Mensagem = $"Erro interno: {ex.Message}"
                });
                return new Result { Status = false, Mensagem = "Erro interno do sistema" };
            }
        }
    }
}
