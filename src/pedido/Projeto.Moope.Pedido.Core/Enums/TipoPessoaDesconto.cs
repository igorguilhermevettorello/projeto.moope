using System.ComponentModel;

namespace Projeto.Moope.Pedido.Core.Enums
{
    public enum TipoPessoaDesconto
    {
        [Description("CPF")]
        CPF = 1,

        [Description("CNPJ")]
        CNPJ = 2
    }
}
