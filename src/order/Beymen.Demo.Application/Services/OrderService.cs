using Beymen.Demo.Application.DTOs;
using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Application.Services;

public class OrderService(
    IUnitOfWork unitOfWork,
    ILogger<OrderService> logger) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<OrderService> _logger = logger;

    public async Task CreateAsync(OrderDto orderDto, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var order = Order.Create();
            order.OrderItems = orderDto.OrderItems.Select(x => new OrderItem(order.Id, x.ProductId, x.Quantity)).ToList();
            order.TotalQuantityCount = order.OrderItems.Count;

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured creating order.");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new InvalidOperationException("An error occured creating order.", ex);
        }

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var order = await _unitOfWork.Orders.GetAsync(id, cancellationToken) ?? throw new InvalidOperationException(nameof(Order) + " cannot found.");
            order.MarkAsDeleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured deleting order.");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new InvalidOperationException("An error occured deleting order.", ex);
        }
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken) =>
        (await _unitOfWork.Orders.GetAllAsync(cancellationToken)).Select(
            o => new OrderDto(
                o.Id,
                o.OrderItems!.Select(oi => new OrderItemDto(oi.Id, oi.ProductId, oi.Quantity, oi.CreatedAt, oi.IsDeleted)).ToList(),
                o.CreatedAt,
                o.IsDeleted
            )
        ).ToList();

    public async Task<OrderDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetAsync(id, cancellationToken) ?? throw new InvalidOperationException(nameof(Order) + " cannot found.");
        return new OrderDto(
            id,
            order.OrderItems!.Select(oi => new OrderItemDto(oi.Id, oi.ProductId, oi.Quantity, oi.CreatedAt, oi.IsDeleted)).ToList(),
            order.CreatedAt,
            order.IsDeleted
        );
    }

    public async Task<IReadOnlyList<OrderItemDto>> GetOrderItems(Guid id, CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Orders.AnyAsync(id, cancellationToken)) throw new InvalidOperationException(nameof(Order) + " cannot found.");

        var orderItems = await _unitOfWork.OrderItems.GetOrderItemsAsync(id, cancellationToken);
        return orderItems.Select(oi => new OrderItemDto(oi.Id, oi.ProductId, oi.Quantity, oi.CreatedAt, oi.IsDeleted)).ToList();
    }

    public async Task UpdateAsync(OrderDto orderDto, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var order = await _unitOfWork.Orders.GetAsync(orderDto.OrderId, cancellationToken) ?? throw new InvalidOperationException(nameof(Order) + " cannot found.");
            order.OrderItems = orderDto.OrderItems.Select(oi => new OrderItem(order.Id, oi.ProductId, oi.Quantity)).ToList();
            order.TotalQuantityCount = order.OrderItems.Count;
            if (orderDto.IsDeleted) order.MarkAsDeleted();
            order.SetUpdatedAt();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured updating order.");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new InvalidOperationException("An error occured updating order.", ex);
        }
    }
}