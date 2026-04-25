namespace Projeto.Moope.Pedido.Core.Excecoes.Idempotencia
{
    public class RegistroIdempotenciaNaoEncontradoException : Exception
    {
        public RegistroIdempotenciaNaoEncontradoException(string mensagem) : base(mensagem) { }
    }
}

