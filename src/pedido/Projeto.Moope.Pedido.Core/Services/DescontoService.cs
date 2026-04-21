using Microsoft.Extensions.Logging;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Pedido.Core.Enums;
using Projeto.Moope.Pedido.Core.Interfaces.Repositories;
using Projeto.Moope.Pedido.Core.Interfaces.Services;
using Projeto.Moope.Pedido.Core.Models;

namespace Projeto.Moope.Pedido.Core.Services
{
    public class DescontoService : IDescontoService
    {
        private readonly IDescontoRepository _descontoRepository;
        private readonly ILogger<DescontoService> _logger;

        public DescontoService(IDescontoRepository descontoRepository, ILogger<DescontoService> logger)
        {
            _descontoRepository = descontoRepository;
            _logger = logger;
        }

        public async Task<ResultDto> SalvarDescontosAsync(Guid pedidoId, IEnumerable<string> codigosDesconto, TipoPessoa tipoPessoa, decimal valorTotal)
        {
            try
            {
                var descontosPredefinidos = DescontoPredefinido.DescontosPredefinidos;

                foreach (var codigoDesconto in codigosDesconto)
                {
                    var descontoPredefinido = descontosPredefinidos.FirstOrDefault(d =>
                        d.Codigo.ToString() == codigoDesconto ||
                        d.Descricao.ToUpper().Replace(" ", "_") == codigoDesconto.ToUpper());

                    if (descontoPredefinido == null)
                    {
                        _logger.LogWarning("Desconto {Codigo} não encontrado nos descontos predefinidos", codigoDesconto);
                        continue;
                    }

                    var tipoPessoaDesconto = tipoPessoa == TipoPessoa.FISICA ? TipoPessoaDesconto.CPF : TipoPessoaDesconto.CNPJ;
                    if (descontoPredefinido.TipoPermitido != tipoPessoaDesconto)
                    {
                        _logger.LogWarning("Desconto {Codigo} não é compatível com o tipo de pessoa {TipoPessoa}", codigoDesconto, tipoPessoa);
                        continue;
                    }

                    var valorDesconto = Math.Round((valorTotal * descontoPredefinido.Valor) / 100, 2);

                    var desconto = new Desconto
                    {
                        PedidoId = pedidoId,
                        CodigoDesconto = codigoDesconto,
                        ValorPercentual = descontoPredefinido.Valor,
                        Descricao = descontoPredefinido.Descricao,
                        TipoPessoa = tipoPessoaDesconto,
                        ValorDesconto = valorDesconto,
                        Ativo = true
                    };

                    await _descontoRepository.SalvarAsync(desconto);

                    _logger.LogInformation("Desconto aplicado ao pedido {PedidoId}: {Codigo} - {ValorPercentual}% - R$ {ValorDesconto}",
                        pedidoId, codigoDesconto, descontoPredefinido.Valor, valorDesconto);
                }

                return new ResultDto
                {
                    Status = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar descontos para o pedido {PedidoId}", pedidoId);
                return new ResultDto
                {
                    Status = false,
                    Mensagem = "Erro ao salvar descontos"
                };
            }
        }

        public decimal ObterPercentualTotalDescontosAsync(List<string> codigosDesconto, TipoPessoa tipoPessoa)
        {
            var percentualDesconto = 0M;

            var descontosPredefinidos = DescontoPredefinido.DescontosPredefinidos;

            foreach (var codigoDesconto in codigosDesconto)
            {
                var descontoPredefinido = descontosPredefinidos.FirstOrDefault(d => d.Codigo.ToString() == codigoDesconto);

                if (descontoPredefinido == null)
                {
                    _logger.LogWarning("Desconto {Codigo} não encontrado nos descontos predefinidos", codigoDesconto);
                    continue;
                }

                var tipoPessoaDesconto = tipoPessoa == TipoPessoa.FISICA ? TipoPessoaDesconto.CPF : TipoPessoaDesconto.CNPJ;
                if (descontoPredefinido.TipoPermitido != tipoPessoaDesconto)
                {
                    _logger.LogWarning("Desconto {Codigo} não é compatível com o tipo de pessoa {TipoPessoa}", codigoDesconto, tipoPessoa);
                    continue;
                }

                percentualDesconto += descontoPredefinido.Valor;

            }

            return percentualDesconto;
        }

        public async Task<decimal> ObterPercentualTotalDescontosAsync(Guid pedidoId)
        {
            var descontos = await _descontoRepository.BuscarPorPedidoIdAsync(pedidoId);
            return descontos?.Where(d => d.Ativo).Sum(d => d.ValorPercentual) ?? 0m;
        }

        public async Task<IEnumerable<Desconto>> BuscarPorPedidoIdAsync(Guid pedidoId)
        {
            return await _descontoRepository.BuscarPorPedidoIdAsync(pedidoId);
        }
    }
}
