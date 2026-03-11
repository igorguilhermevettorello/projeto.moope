using MediatR;

namespace Projeto.Moope.Core.Interfaces.Commands
{
    public interface ICommand : IRequest
    {
    }

    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}
