namespace Projeto.Moope.Pedido.Core.Excecoes.Idempotencia
{
    public class ChaveIdempotenteReutilizadaComPayloadDiferenteException : Exception
    {
        public ChaveIdempotenteReutilizadaComPayloadDiferenteException(string mensagem) : base(mensagem) { }
    }
}

