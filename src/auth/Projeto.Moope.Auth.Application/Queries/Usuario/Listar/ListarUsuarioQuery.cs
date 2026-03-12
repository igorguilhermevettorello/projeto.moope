using MediatR;
using Projeto.Moope.Auth.Core.DTOs.Usuario;

namespace Projeto.Moope.Auth.Application.Queries.Usuario.Listar
{
    public class ListarUsuarioQuery : IRequest<IEnumerable<ListarUsuarioDto>>
    {
    }
}
