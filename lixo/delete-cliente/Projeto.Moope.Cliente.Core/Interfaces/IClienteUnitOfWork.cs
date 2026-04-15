namespace Projeto.Moope.Cliente.Core.Interfaces
{
    public interface IClienteUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
