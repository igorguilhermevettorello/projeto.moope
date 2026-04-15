namespace Projeto.Moope.Gateways.Core.Models
{
    public sealed class CadastroClienteCompostoOutput
    {
        public Guid ClienteId { get; set; }

        public Guid UsuarioId { get; set; }

        public Guid EnderecoId { get; set; }
    }
}
