namespace Projeto.Moope.Auth.Core.Events
{
    public class UserCreated
    {
        public Guid UserId { get; }
        public string Nome { get; }
        public string Email { get; }

        public UserCreated(Guid userId, string nome, string email)
        {
            UserId = userId;
            Nome = nome;
            Email = email;
        }
    }
}