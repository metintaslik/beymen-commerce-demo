namespace Beymen.Demo.Application.DTOs;

public record OrderItemDto(int OrderItemId, Guid ProductId, int Quantity, DateTime CreatedAt, bool IsDeleted);
