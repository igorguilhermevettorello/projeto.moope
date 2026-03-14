using System.ComponentModel;

namespace Projeto.Moope.Core.Enums
{
    public enum ComodatoConviteStatus
    {
        [Description("Criado")]
        Criado = 1,
        [Description("Aberto")]
        Aberto = 2,
        [Description("Consumido")]
        Consumido = 3,
        [Description("Expirado")]
        Expirado = 4,
        [Description("Cancelado")]
        Cancelado = 5
    }
}
