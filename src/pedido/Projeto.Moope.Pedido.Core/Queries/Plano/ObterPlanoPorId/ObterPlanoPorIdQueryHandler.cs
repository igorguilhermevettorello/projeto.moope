using MediatR;
using Projeto.Moope.Pedido.Core.DTOs.Plano;
using Projeto.Moope.Pedido.Core.Interfaces.Gateways;

namespace Projeto.Moope.Pedido.Core.Queries.Plano.ObterPlanoPorId
{
    public class ObterPlanoPorIdQueryHandler : IRequestHandler<ObterPlanoPorIdQuery, PlanoDetailDto?>
    {
        private readonly IPlanoReadGateway _planoReadGateway;

        public ObterPlanoPorIdQueryHandler(IPlanoReadGateway planoReadGateway)
        {
            _planoReadGateway = planoReadGateway;
        }

        public Task<PlanoDetailDto?> Handle(ObterPlanoPorIdQuery request, CancellationToken cancellationToken)
        {
            if (request.PlanoId == Guid.Empty)
                return Task.FromResult<PlanoDetailDto?>(null);

            return _planoReadGateway.ObterPorIdAsync(request.PlanoId, cancellationToken);
        }
    }
}
