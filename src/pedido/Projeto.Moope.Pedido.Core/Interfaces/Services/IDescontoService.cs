using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Pedido.Core.Models;

namespace Projeto.Moope.Pedido.Core.Interfaces.Services
{
    public interface IDescontoService
    {
        Task<ResultDto> SalvarDescontosAsync(Guid pedidoId, IEnumerable<string> codigosDesconto, TipoPessoa tipoPessoa, decimal valorTotal);
        Task<decimal> ObterPercentualTotalDescontosAsync(Guid pedidoId);
        decimal ObterPercentualTotalDescontosAsync(List<string> codigosDesconto, TipoPessoa tipoPessoa);
        Task<IEnumerable<Desconto>> BuscarPorPedidoIdAsync(Guid pedidoId);
    }
}
