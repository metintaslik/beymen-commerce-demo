using Beymen.Demo.Domain.Enums;

namespace Beymen.Demo.Application.DTOs;

public record UpdateQuantityDto(Guid ProductId, int Quantity, QuantityProcessType QuantityProcessType);