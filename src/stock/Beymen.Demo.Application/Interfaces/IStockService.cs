using Beymen.Demo.Application.DTOs;

namespace Beymen.Demo.Application.Interfaces;

public interface IStockService
{
    Task UpdateQuantityAsync(UpdateQuantityDto? updateQuantityDto, CancellationToken stoppingToken);
}