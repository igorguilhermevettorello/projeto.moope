namespace Projeto.Moope.Core.Interfaces.Data
{
    public interface IRepository<T> : IDisposable where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
        Task<T> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<T>> BuscarTodosAsync();
        Task<T> SalvarAsync(T entity);
        Task<T> AtualizarAsync(T entity);
        Task<bool> RemoverAsync(Guid id);
    }
}
