namespace Projeto.Moope.Pagamento.Core.Configurations
{
    public class IntencaoPagamentoOptions
    {
        public const string SectionName = "IntencaoPagamento";

        /// <summary>
        /// Tempo de vida da intenção a partir da criação.
        /// </summary>
        public int MinutosValidade { get; set; } = 30;
    }
}
