using Beymen.Demo.Domain.Interfaces;
using Beymen.Demo.Infrastructure.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Infrastructure.Persistance.Data;

public class UnitOfWork(
    OrderDbContext dbContext,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private readonly OrderDbContext db = dbContext;
    private readonly ILogger<UnitOfWork> _logger = logger;

    private bool _disposed;

    public IOrderRepository Orders => serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<OrderRepository>();
    public IOrderItemRepository OrderItems => serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<OrderItemRepository>();

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return (await db.SaveChangesAsync(cancellationToken)) > 0;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes");
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        await db.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await db.Database.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while committing transaction");
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await db.Database.RollbackTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rolling back transaction");
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            db.Dispose();
        }
        _disposed = true;
    }
}