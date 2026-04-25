namespace Projeto.Moope.RabbitMQ.Core.DTOs.Usuario
{
    public sealed class ClientePendenteDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}

