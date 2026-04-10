namespace Projeto.Moope.Cliente.Core.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<T>> BuscarTodosAsync();
        Task<T> SalvarAsync(T entity);
        Task<T> AtualizarAsync(T entity);
        Task<bool> RemoverAsync(Guid id);
    }
}
