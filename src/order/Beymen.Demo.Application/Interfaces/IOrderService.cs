using Beymen.Demo.Application.DTOs;

namespace Beymen.Demo.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken);
    Task CreateAsync(OrderDto orderDto, CancellationToken cancellationToken);
    Task UpdateAsync(OrderDto orderDto, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderItemDto>> GetOrderItems(Guid id, CancellationToken cancellationToken);
}