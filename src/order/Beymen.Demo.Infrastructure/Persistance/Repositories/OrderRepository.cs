using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Beymen.Demo.Infrastructure.Persistance.Repositories;

public class OrderRepository(
    OrderDbContext dbContext) : IOrderRepository
{
    private readonly OrderDbContext db = dbContext;
    private readonly DbSet<Order> _orders = dbContext.Set<Order>();

    public async Task<bool> AnyAsync(Guid id, CancellationToken cancellationToken) =>
        await _orders.AnyAsync(o => o.Id == id, cancellationToken);

    public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _orders.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Delete(Order entity)
    {
        entity.MarkAsDeleted();
        db.Entry(entity).State = EntityState.Modified;
        db.Update(entity);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _orders.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _orders.AsNoTracking().Include(x => x.OrderItems).SingleOrDefaultAsync(cancellationToken);

    public void Update(Order entity)
    {
        entity.SetUpdatedAt();
        db.Entry(entity).State = EntityState.Modified;
        db.Update(entity);
    }
}