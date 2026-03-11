using MediatR;
using FluentValidation.Results;

namespace Projeto.Moope.Core.Messages
{
    public abstract class Command<TResponse> : Message, IRequest<TResponse>
    {
        public DateTime Timestamp { get; set; }
        public ValidationResult ValidationResult { get; set; }

        protected Command()
        {
            Timestamp = DateTime.Now;
        }

        public abstract bool IsValid();
    }
}
