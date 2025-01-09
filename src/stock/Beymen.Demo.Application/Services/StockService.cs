using Beymen.Demo.Application.DTOs;
using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Enums;
using Beymen.Demo.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Application.Services;

public class StockService(
    IUnitOfWork unitOfWork,
    ILogger<StockService> logger) : IStockService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<StockService> _logger = logger;

    public async Task UpdateQuantityAsync(UpdateQuantityDto? updateQuantityDto, CancellationToken stoppingToken)
    {
        var stock = await _unitOfWork.Stocks.GetByProductIdAsync(updateQuantityDto!.ProductId, stoppingToken);
        if (stock is null) throw new InvalidOperationException(nameof(stock) + " cannot found.");

        var newQuantity = CalculateNewStock(stock.Quantity, updateQuantityDto.Quantity, updateQuantityDto.QuantityProcessType);
        stock.UpdateQuantity(newQuantity);

        await _unitOfWork.BeginTransactionAsync(stoppingToken);
        await _unitOfWork.SaveChangesAsync(stoppingToken);
        await _unitOfWork.CommitTransactionAsync(stoppingToken);

        _logger.LogDebug("Product {ProductId} stock quantity updated.", updateQuantityDto.ProductId);
        await Task.CompletedTask;
    }

    private static int CalculateNewStock(int stockQuantity, int requestQuantity, QuantityProcessType quantityProcessType) =>
        quantityProcessType switch
        {
            QuantityProcessType.Increase => stockQuantity + requestQuantity,
            QuantityProcessType.Decrease => stockQuantity - requestQuantity,
            _ => throw new ArgumentOutOfRangeException(nameof(quantityProcessType), "Invalid quantity process type.")
        };
}