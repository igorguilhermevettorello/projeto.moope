using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Pagamento.Core.Configurations;
using Projeto.Moope.Pagamento.Core.DTOs.Intencao;
using Projeto.Moope.Pagamento.Core.Enums;
using Projeto.Moope.Pagamento.Core.Interfaces.Repositories;
using Projeto.Moope.Pagamento.Core.Models;
using Projeto.Moope.Pagamento.Core.Services;
using Xunit;

namespace Projeto.Moope.Pagamento.Core.Tests.Services
{
    public class IntencaoPagamentoServiceTests
    {
        private readonly Mock<IIntencaoPagamentoRepository> repositorio = new();
        private readonly Mock<INotificador> notificador = new();
        private readonly IOptions<IntencaoPagamentoOptions> opcoes = Options.Create(
            new IntencaoPagamentoOptions { MinutosValidade = 20 });

        private IntencaoPagamentoService CriarServico()
        {
            return new IntencaoPagamentoService(
                repositorio.Object,
                opcoes,
                notificador.Object,
                NullLogger<IntencaoPagamentoService>.Instance);
        }

        [Fact]
        public async Task CriarAsync_ValorNaoPositivo_NaoChamaRepositorio()
        {
            repositorio
                .Setup(x => x.AdicionarAsync(It.IsAny<IntencaoPagamento>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("não deve ser chamado"));

            var servico = CriarServico();
            var resposta = await servico.CriarAsync(
                new CriarIntencaoPagamentoRequestDto
                {
                    Valor = 0m,
                    Moeda = "BRL",
                    MetodoPagamento = MetodoPagamento.PIX
                });

            Assert.Null(resposta);
            repositorio.Verify(
                x => x.AdicionarAsync(It.IsAny<IntencaoPagamento>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CriarAsync_Sucesso_PersisteCriadaETtl()
        {
            IntencaoPagamento? recebida = null;
            repositorio
                .Setup(x => x.AdicionarAsync(It.IsAny<IntencaoPagamento>(), It.IsAny<CancellationToken>()))
                .Callback<IntencaoPagamento, CancellationToken>((e, _) => recebida = e)
                .Returns((IntencaoPagamento e, CancellationToken _) => Task.FromResult(e));

            var antes = DateTime.UtcNow;
            var servico = CriarServico();
            var resposta = await servico.CriarAsync(
                new CriarIntencaoPagamentoRequestDto
                {
                    Valor = 10.50m,
                    Moeda = " brl ",
                    MetodoPagamento = MetodoPagamento.CreditCard
                });

            Assert.NotNull(resposta);
            Assert.NotEqual(Guid.Empty, resposta.Id);
            Assert.Equal("BRL", resposta.Moeda);
            Assert.Equal(StatusIntencaoPagamento.Criada, resposta.Status);
            Assert.Equal(MetodoPagamento.CreditCard, resposta.MetodoPagamento);
            Assert.Equal(10.50m, resposta.Valor);

            Assert.NotNull(recebida);
            var esperadoAte = antes.AddMinutes(20);
            Assert.True(
                resposta.ExpiresAt >= antes.AddMinutes(20).AddSeconds(-2)
                && resposta.ExpiresAt <= esperadoAte.AddSeconds(2),
                "ExpiresAt deve refletir o TTL em minutos configurado.");
        }

        [Fact]
        public async Task CriarAsync_MoedaTamanhoInvalido_RetornaNull()
        {
            var servico = CriarServico();
            var resposta = await servico.CriarAsync(
                new CriarIntencaoPagamentoRequestDto
                {
                    Valor = 1m,
                    Moeda = "BR",
                    MetodoPagamento = MetodoPagamento.PIX
                });

            Assert.Null(resposta);
        }

        [Fact]
        public async Task CriarAsync_MetodoNaoDefinidoNoEnum_RetornaNull()
        {
            // Valor numérico fora do enum — Enum.IsDefined retorna false
            const MetodoPagamento naoExiste = (MetodoPagamento)999;
            var servico = CriarServico();
            var resposta = await servico.CriarAsync(
                new CriarIntencaoPagamentoRequestDto
                {
                    Valor = 1m,
                    Moeda = "BRL",
                    MetodoPagamento = naoExiste
                });

            Assert.Null(resposta);
        }
    }
}
