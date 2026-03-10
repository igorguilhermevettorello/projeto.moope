namespace Projeto.Moope.Core.Interfaces.Notificacao
{
    public interface INotificador
    {
        bool TemNotificacao();
        List<Notifications.Notificacao> ObterNotificacoes();
        void Handle(Notifications.Notificacao notificacao);
    }
}
