namespace Projeto.Moope.Gateways.Core.Models
{
    public sealed class ProcessarVendaUsuarioContext
    {
        public bool IsAdministrador { get; init; }

        public Guid? UsuarioId { get; init; }
    }
}
