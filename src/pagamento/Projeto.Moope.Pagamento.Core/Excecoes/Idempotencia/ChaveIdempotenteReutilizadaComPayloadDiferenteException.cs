namespace Projeto.Moope.Pagamento.Core.Excecoes.Idempotencia
{
    public class ChaveIdempotenteReutilizadaComPayloadDiferenteException : Exception
    {
        public ChaveIdempotenteReutilizadaComPayloadDiferenteException(string mensagem) : base(mensagem) { }
    }
}

