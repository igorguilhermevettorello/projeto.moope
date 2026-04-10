using MediatR;
using Projeto.Moope.Cliente.Core.Commands.Clientes.AlterarSenha;
using Projeto.Moope.Cliente.Core.Interfaces.Repositories;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Cliente.Core.Models;
using Projeto.Moope.Cliente.Core.Validators;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;
using ClienteModel = Projeto.Moope.Cliente.Core.Models.Cliente;

namespace Projeto.Moope.Cliente.Core.Services
{
    public class ClienteService : BaseService, IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IIdentityUserService _identityUserService;
        private readonly IMediator _mediator;

        public ClienteService(
            IClienteRepository clienteRepository,
            IIdentityUserService identityUserService,
            IMediator mediator,
            INotificador notificador) : base(notificador)
        {
            _clienteRepository = clienteRepository;
            _identityUserService = identityUserService;
            _mediator = mediator;
        }

        public async Task<ClienteModel?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _clienteRepository.BuscarPorIdAsNotrackingAsync(id);
        }

        public async Task<ClienteModel?> BuscarPorIdAsync(Guid id)
        {
            return await _clienteRepository.BuscarPorIdAsync(id);
        }

        public async Task<ClienteModel?> BuscarPorEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Notificar("Email", "Email é obrigatório para busca");
                return null;
            }

            try
            {
                var usuarioId = await _identityUserService.BuscarPorEmailAsync(email);
                if (usuarioId == null || usuarioId == Guid.Empty)
                    return null;

                return await _clienteRepository.BuscarPorIdAsync(usuarioId.Value);
            }
            catch (Exception ex)
            {
                Notificar("Email", $"Erro ao buscar cliente por email: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<ClienteModel>> BuscarTodosAsync()
        {
            return await _clienteRepository.BuscarTodosAsync();
        }

        public async Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>()
        {
            return await _clienteRepository.BuscarClientesComDadosAsync<T>();
        }

        public async Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id)
        {
            return await _clienteRepository.BuscarClientePorIdComDadosAsync<T>(id);
        }

        public async Task<Result<ClienteModel>> SalvarAsync(ClienteModel cliente)
        {
            if (!ExecutarValidacao(new ClienteValidator(), cliente))
                return new Result<ClienteModel> { Status = false, Mensagem = "Dados do cliente são inválidos" };

            await _clienteRepository.SalvarAsync(cliente);
            return new Result<ClienteModel>
            {
                Status = true,
                Dados = cliente,
                Mensagem = "Cliente salvo com sucesso"
            };
        }

        public async Task<Result<ClienteModel>> AtualizarAsync(ClienteModel cliente)
        {
            if (!ExecutarValidacao(new ClienteValidator(), cliente))
                return new Result<ClienteModel> { Status = false, Mensagem = "Dados do cliente são inválidos" };

            try
            {
                var clienteExistente = await _clienteRepository.BuscarPorIdAsync(cliente.Id);
                if (clienteExistente == null)
                {
                    Notificar("Cliente", "Cliente não encontrado");
                    return new Result<ClienteModel> { Status = false, Mensagem = "Cliente não encontrado" };
                }

                var entity = await _clienteRepository.AtualizarAsync(cliente);
                return new Result<ClienteModel>
                {
                    Status = true,
                    Dados = entity,
                    Mensagem = "Cliente atualizado com sucesso"
                };
            }
            catch (Exception ex)
            {
                Notificar("Cliente", $"Erro ao atualizar cliente: {ex.Message}");
                return new Result<ClienteModel> { Status = false, Mensagem = "Erro interno ao atualizar cliente" };
            }
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            try
            {
                var clienteExistente = await _clienteRepository.BuscarPorIdAsync(id);
                if (clienteExistente == null)
                {
                    Notificar("Cliente", "Cliente não encontrado");
                    return false;
                }

                await _clienteRepository.RemoverAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                Notificar("Cliente", $"Erro ao remover cliente: {ex.Message}");
                return false;
            }
        }

        public async Task<Result> AlterarSenhaClienteAsync(Guid clienteId, string senhaAtual, string novaSenha)
        {
            try
            {
                var command = new AlterarSenhaClienteCommand(clienteId, senhaAtual, novaSenha);
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                Notificar("Mensagem", $"Erro ao alterar senha do cliente: {ex.Message}");
                return new Result { Status = false, Mensagem = "Erro interno do sistema" };
            }
        }

        public async Task<Result> AlterarSenhaAdminAsync(Guid clienteId, string novaSenha)
        {
            try
            {
                var command = new AlterarSenhaAdminCommand(clienteId, novaSenha);
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                Notificar("Mensagem", $"Erro ao alterar senha do cliente: {ex.Message}");
                return new Result { Status = false, Mensagem = "Erro interno do sistema" };
            }
        }

        public async Task<Result<bool>> AlterarTelefoneEmergencia(ClienteModel cliente)
        {
            cliente.Updated = DateTime.UtcNow;
            await _clienteRepository.AtualizarAsync(cliente);

            return new Result<bool>
            {
                Status = true,
                Dados = true,
                Mensagem = "Telefone de emergência atualizado"
            };
        }
    }
}
