namespace Beymen.Demo.Application.DTOs;

public record OrderDto(Guid OrderId, IReadOnlyList<OrderItemDto> OrderItems, DateTime CreatedAt, bool IsDeleted);