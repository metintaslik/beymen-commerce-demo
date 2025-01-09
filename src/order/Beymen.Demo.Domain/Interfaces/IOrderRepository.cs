using Beymen.Demo.Domain.Entities;

namespace Beymen.Demo.Domain.Interfaces;

public interface IOrderRepository
{
    Task<bool> AnyAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default);
    void Update(Order entity);
    void Delete(Order entity);
}