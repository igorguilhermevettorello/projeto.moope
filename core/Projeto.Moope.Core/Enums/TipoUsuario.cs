using System.ComponentModel;

namespace Projeto.Moope.Core.Enums
{
    public enum TipoUsuario
    {
        [Description("Administrador")]
        Administrador = 0,
        [Description("Vendedor")]
        Vendedor = 1,
        [Description("Cliente ")]
        Cliente = 2
    }
}
