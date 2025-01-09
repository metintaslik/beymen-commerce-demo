namespace Beymen.Demo.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IStockRepository Stocks { get; }

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}