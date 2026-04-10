namespace Projeto.Moope.Gateways.Core.Models
{
    /// <summary>
    /// Claims do usuario autenticado (opcional — checkout pode ser anonimo).
    /// </summary>
    public sealed class ProcessarVendaUsuarioContext
    {
        public bool IsAdministrador { get; init; }

        public Guid? UsuarioId { get; init; }
    }
}
