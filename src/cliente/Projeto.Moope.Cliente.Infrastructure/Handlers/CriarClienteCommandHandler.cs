using MediatR;
using Projeto.Moope.Cliente.Core.Commands.Clientes.Criar;
using Projeto.Moope.Cliente.Core.Interfaces;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Cliente.Core.Models;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Infrastructure.Handlers
{
    public class CriarClienteCommandHandler : IRequestHandler<CriarClienteCommand, Result<Guid>>
    {
        private readonly IClienteService _clienteService;
        private readonly IIdentityUserService _identityUserService;
        private readonly IClienteUnitOfWork _unitOfWork;
        private readonly INotificador _notificador;

        public CriarClienteCommandHandler(
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

        public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
        {
            var clienteId = Guid.NewGuid();
            var usuarioExistente = false;
            Guid? identityUserCriadoId = null;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var cliente = CriarCliente(request);

                var rsIdentity = await _identityUserService.CriarUsuarioAsync(
                    request.Email,
                    request.Senha,
                    telefone: request.Telefone,
                    tipoUsuario: TipoUsuario.Cliente);

                usuarioExistente = rsIdentity.UsuarioExiste;

                if (!rsIdentity.Status)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Mensagem",
                        Mensagem = rsIdentity.Mensagem ?? "Falha ao criar usuário no sistema"
                    });

                    await _unitOfWork.RollbackAsync();

                    return new Result<Guid>
                    {
                        Status = false,
                        Mensagem = rsIdentity.Mensagem ?? "Falha ao criar usuário no sistema"
                    };
                }

                identityUserCriadoId = rsIdentity.Dados;

                if (rsIdentity.UsuarioExiste)
                {
                    var clienteExistente = await _clienteService.BuscarPorIdAsync(identityUserCriadoId!.Value);
                    if (clienteExistente != null)
                    {
                        throw new Exception("Cliente já cadastrado para este usuário.");
                    }
                }

                clienteId = rsIdentity.Dados;
                cliente.Id = clienteId;
                var rsCliente = await _clienteService.SalvarAsync(cliente);
                if (!rsCliente.Status)
                    throw new Exception(rsCliente.Mensagem ?? "Falha ao salvar cliente");

                await _unitOfWork.CommitAsync();

                return new Result<Guid>
                {
                    Status = true,
                    Dados = clienteId,
                    Mensagem = "Cliente criado com sucesso"
                };
            }
            catch (Exception ex)
            {
                if (!usuarioExistente && identityUserCriadoId.HasValue)
                {
                    await _identityUserService.RemoverAoFalharAsync(identityUserCriadoId.Value);
                }

                _notificador.Handle(new Notificacao
                {
                    Campo = "Mensagem",
                    Mensagem = ex.Message
                });

                await _unitOfWork.RollbackAsync();

                return new Result<Guid>
                {
                    Status = false,
                    Mensagem = ex.Message
                };
            }
        }

        private static ClienteModel CriarCliente(CriarClienteCommand request)
        {
            return new ClienteModel
            {
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                VendedorId = request.VendedorId,
                Telefone = request.Telefone ?? string.Empty,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

    }
}
