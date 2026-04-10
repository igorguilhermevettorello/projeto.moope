using MediatR;
using Projeto.Moope.Cliente.Core.Commands.Clientes.Atualizar;
using Projeto.Moope.Cliente.Core.Interfaces;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Cliente.Core.Models;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Infrastructure.Handlers
{
    public class AtualizarClienteCommandHandler : IRequestHandler<AtualizarClienteCommand, Result<bool>>
    {
        private readonly IClienteService _clienteService;
        private readonly IIdentityUserService _identityUserService;
        private readonly IClienteUnitOfWork _unitOfWork;
        private readonly INotificador _notificador;

        public AtualizarClienteCommandHandler(
            IClienteService clienteService,
            IIdentityUserService identityUserService,
            IClienteUnitOfWork unitOfWork,
            INotificador notificador)
        {
            _clienteService = clienteService;
            _identityUserService = identityUserService;
            _unitOfWork = unitOfWork;
            _notificador = notificador;
        }

        public async Task<Result<bool>> Handle(AtualizarClienteCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var clienteExistente = await _clienteService.BuscarPorIdAsNotrackingAsync(request.Id);
                if (clienteExistente == null)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Cliente",
                        Mensagem = "Cliente não encontrado"
                    });
                    await _unitOfWork.RollbackAsync();
                    return new Result<bool>
                    {
                        Status = false,
                        Mensagem = "Cliente não encontrado"
                    };
                }

                var rsIdentity = await _identityUserService.AlterarUsuarioAsync(
                    request.Id,
                    request.Email,
                    telefone: request.Telefone);

                if (!rsIdentity.Status)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Identity",
                        Mensagem = rsIdentity.Mensagem ?? "Erro ao atualizar usuário no Identity"
                    });
                    await _unitOfWork.RollbackAsync();
                    return new Result<bool>
                    {
                        Status = false,
                        Mensagem = rsIdentity.Mensagem ?? "Erro ao atualizar usuário no Identity"
                    };
                }
                
                var cliente = CriarCliente(request);
                var rsCliente = await _clienteService.AtualizarAsync(cliente);
                if (!rsCliente.Status)
                    throw new Exception(rsCliente.Mensagem ?? "Erro ao atualizar cliente");

                await _unitOfWork.CommitAsync();

                return new Result<bool>
                {
                    Status = true,
                    Dados = true,
                    Mensagem = "Cliente atualizado com sucesso"
                };
            }
            catch (Exception ex)
            {
                _notificador.Handle(new Notificacao
                {
                    Campo = "Erro",
                    Mensagem = ex.Message
                });

                await _unitOfWork.RollbackAsync();

                return new Result<bool>
                {
                    Status = false,
                    Mensagem = ex.Message
                };
            }
        }

        private static ClienteModel CriarCliente(AtualizarClienteCommand request)
        {
            return new ClienteModel
            {
                Id = request.Id,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                VendedorId = request.VendedorId,
                Updated = DateTime.UtcNow
            };
        }
    }
}
