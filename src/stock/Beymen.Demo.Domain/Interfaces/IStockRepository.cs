using Beymen.Demo.Domain.Entities;

namespace Beymen.Demo.Domain.Interfaces;

public interface IStockRepository
{
    Task<IReadOnlyList<Stock>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Stock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Stock> AddAsync(Stock entity, CancellationToken cancellationToken = default);
    void Update(Stock entity);
    void Delete(Stock entity);
}