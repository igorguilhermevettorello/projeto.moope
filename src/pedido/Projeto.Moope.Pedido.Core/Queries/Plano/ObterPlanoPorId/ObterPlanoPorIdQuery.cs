using MediatR;
using Projeto.Moope.Pedido.Core.DTOs.Plano;

namespace Projeto.Moope.Pedido.Core.Queries.Plano.ObterPlanoPorId
{
    public class ObterPlanoPorIdQuery : IRequest<PlanoDetailDto?>
    {
        public Guid PlanoId { get; init; }
    }
}
