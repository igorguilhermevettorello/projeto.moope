using MediatR;
using Projeto.Moope.Auth.Core.DTOs.Usuario;

namespace Projeto.Moope.Auth.Application.Queries.Usuario.ObterPorId
{
    public class ObterUsuarioPorIdQuery : IRequest<UsuarioDetalheDto?>
    {
        public Guid Id { get; set; }
    }
}
