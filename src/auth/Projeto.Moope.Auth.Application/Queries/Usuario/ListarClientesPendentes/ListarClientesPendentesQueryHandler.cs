using MediatR;
using Projeto.Moope.Auth.Core.DTOs.Usuario;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;

namespace Projeto.Moope.Auth.Application.Queries.Usuario.ListarClientesPendentes
{
    public class ListarClientesPendentesQueryHandler : IRequestHandler<ListarClientesPendentesQuery, IReadOnlyList<ClientePendenteDto>>
    {
        private readonly IClientePendenteRepository _clientePendenteRepository;

        public ListarClientesPendentesQueryHandler(IClientePendenteRepository clientePendenteRepository)
        {
            _clientePendenteRepository = clientePendenteRepository;
        }

        public async Task<IReadOnlyList<ClientePendenteDto>> Handle(ListarClientesPendentesQuery request, CancellationToken cancellationToken)
        {
            return await _clientePendenteRepository.ListarClientesPendentesAsync(cancellationToken);
        }
    }
}

