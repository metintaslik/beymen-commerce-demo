using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Beymen.Demo.Infrastructure.Persistance.Repositories;

public class OrderItemRepository(
    OrderDbContext dbContext) : IOrderItemRepository
{
    private readonly OrderDbContext db = dbContext;
    private readonly DbSet<OrderItem> _orderItems = dbContext.Set<OrderItem>();

    public async Task<OrderItem> AddAsync(OrderItem entity, CancellationToken cancellationToken = default)
    {
        await _orderItems.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Delete(OrderItem entity)
    {
        entity.MarkAsDeleted();
        db.Entry(entity).State = EntityState.Modified;
        db.Update(entity);
    }

    public async Task<OrderItem?> GetOrderItemAsync(int id, CancellationToken cancellationToken = default) =>
        await _orderItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OrderItem>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await _orderItems.AsNoTracking().Where(x => x.OrderId == orderId).ToListAsync(cancellationToken);

    public void Update(OrderItem entity)
    {
        entity.SetUpdatedAt();
        db.Entry(entity).State = EntityState.Modified;
        db.Update(entity);
    }
}