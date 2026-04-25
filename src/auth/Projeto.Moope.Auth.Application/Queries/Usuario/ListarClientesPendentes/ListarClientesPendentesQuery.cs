using MediatR;
using Projeto.Moope.Auth.Core.DTOs.Usuario;

namespace Projeto.Moope.Auth.Application.Queries.Usuario.ListarClientesPendentes
{
    public class ListarClientesPendentesQuery : IRequest<IReadOnlyList<ClientePendenteDto>>
    {
    }
}

