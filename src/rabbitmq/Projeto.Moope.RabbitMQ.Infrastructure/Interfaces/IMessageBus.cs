using System.Threading.Tasks;

namespace Projeto.Moope.RabbitMQ.Infrastructure.Interfaces
{
    public interface IMessageBus
    {
        Task Publish<T>(T message) where T : class;
    }
}