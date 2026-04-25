namespace Projeto.Moope.Pedido.Core.Excecoes.Idempotencia
{
    public class InconsistenciaEstadoIdempotenciaException : Exception
    {
        public InconsistenciaEstadoIdempotenciaException(string mensagem) : base(mensagem) { }
    }
}

