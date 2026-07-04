using Microsoft.AspNetCore.Http;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Pedido.Core.DTOs.Pedido;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Core.Interfaces.Services;

namespace Projeto.Moope.Pedido.Core.Services
{
    public class PedidoListService : IPedidoListService
    {
        private readonly IPedidoRepository _pedidoRepository;

        public PedidoListService(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        public async Task<ResultDto<IReadOnlyList<PedidoListItemDto>>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            var pedidos = await _pedidoRepository.ListarAsync(cancellationToken);

            var itens = pedidos.Select(p => new PedidoListItemDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                VendedorId = p.VendedorId,
                Plano = p.PlanoDescricao,
                Valor = p.PlanoValor,
                Quantidade = p.Quantidade,
                Total = p.PlanoValorTotal
            }).ToList();

            return new ResultDto<IReadOnlyList<PedidoListItemDto>>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Dados = itens
            };
        }
    }
}
