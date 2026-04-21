namespace Projeto.Moope.Pedido.Core.Enums
{
    public class DescontoPredefinido
    {
        public int Codigo { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public int QuantidadeMinima { get; set; }
        public TipoPessoaDesconto TipoPermitido { get; set; }

        public static readonly DescontoPredefinido[] DescontosPredefinidos = new DescontoPredefinido[]
        {
            new DescontoPredefinido
            {
                Codigo = 1,
                Valor = 10,
                Descricao = "Promoção Especial",
                QuantidadeMinima = 1,
                TipoPermitido = TipoPessoaDesconto.CPF
            },
            new DescontoPredefinido
            {
                Codigo = 2,
                Valor = 10,
                Descricao = "Sou motorista de app/profissional",
                QuantidadeMinima = 1,
                TipoPermitido = TipoPessoaDesconto.CPF
            },
            new DescontoPredefinido
            {
                Codigo = 3,
                Valor = 10,
                Descricao = "Frota de 1 a 5 veículos",
                QuantidadeMinima = 1,
                TipoPermitido = TipoPessoaDesconto.CNPJ
            },
            new DescontoPredefinido
            {
                Codigo = 4,
                Valor = 15,
                Descricao = "Frota de 6 a 10 veículos",
                QuantidadeMinima = 6,
                TipoPermitido = TipoPessoaDesconto.CNPJ
            },
            new DescontoPredefinido
            {
                Codigo = 5,
                Valor = 20,
                Descricao = "Frota de mais de 11 veículos",
                QuantidadeMinima = 11,
                TipoPermitido = TipoPessoaDesconto.CNPJ
            }
        };

        public static DescontoPredefinido? BuscarPorCodigo(int codigo)
        {
            return DescontosPredefinidos.FirstOrDefault(d => d.Codigo == codigo);
        }

        public static bool ExisteCodigo(int codigo)
        {
            return DescontosPredefinidos.Any(d => d.Codigo == codigo);
        }

        public static IEnumerable<DescontoPredefinido> BuscarPorTipoPessoa(TipoPessoaDesconto tipoPessoa)
        {
            return DescontosPredefinidos.Where(d => d.TipoPermitido == tipoPessoa);
        }
    }
}
