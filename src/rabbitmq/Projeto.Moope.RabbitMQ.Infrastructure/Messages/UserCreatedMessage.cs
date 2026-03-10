namespace Projeto.Moope.RabbitMQ.Infrastructure.Messages
{
    public class UserCreatedMessage
    {
        public Guid UserId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }
}