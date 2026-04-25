namespace Projeto.Moope.Pagamento.Core.Interfaces.Services
{
    public interface IGeradorHashRequisicao
    {
        string GerarHash(object requisicao);
    }
}

