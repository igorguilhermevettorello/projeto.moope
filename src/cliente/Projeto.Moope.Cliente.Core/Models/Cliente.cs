namespace Projeto.Moope.Cliente.Core.Models
{
    public class Cliente
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }
        public DateTime DataCadastro { get; private set; }

        // Relacionamento com Usuario do BC Auth
        public Guid UsuarioId { get; private set; }

        protected Cliente() { }

        public Cliente(Guid usuarioId, string nome, string email)
        {
            Id = Guid.NewGuid();
            UsuarioId = usuarioId;
            Nome = nome;
            Email = email;
            DataCadastro = DateTime.UtcNow;
        }
    }
}