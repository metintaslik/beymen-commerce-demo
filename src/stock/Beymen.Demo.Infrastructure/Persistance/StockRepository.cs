using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Beymen.Demo.Infrastructure.Persistance;

public class StockRepository(
    StockDbContext dbContext) : IStockRepository
{
    private readonly StockDbContext db = dbContext;
    private readonly DbSet<Stock> _stocks = dbContext.Set<Stock>();

    public async Task<Stock> AddAsync(Stock entity, CancellationToken cancellationToken = default)
    {
        await _stocks.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Delete(Stock entity)
    {
        entity.MarkAsDeleted();
        db.Entry(entity).State = EntityState.Modified;
        db.Update(entity);
    }

    public async Task<IReadOnlyList<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _stocks.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Stock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _stocks.AsNoTracking().SingleOrDefaultAsync(s => s.ProductId == productId, cancellationToken);
    }

    public void Update(Stock entity)
    {
        entity.SetUpdatedAt();
        db.Entry(entity).State = EntityState.Modified;
        db.Update(entity);
    }
}