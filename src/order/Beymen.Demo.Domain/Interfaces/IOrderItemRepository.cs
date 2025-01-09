using Beymen.Demo.Domain.Entities;

namespace Beymen.Demo.Domain.Interfaces;

public interface IOrderItemRepository
{
    Task<IReadOnlyList<OrderItem>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderItem?> GetOrderItemAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderItem> AddAsync(OrderItem entity, CancellationToken cancellationToken = default);
    void Update(OrderItem entity);
    void Delete(OrderItem entity);
}