using Beymen.Demo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Infrastructure.Persistance;

public class UnitOfWork(
    StockDbContext dbContext,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private readonly StockDbContext db = dbContext;
    private readonly ILogger<UnitOfWork> _logger = logger;

    private bool _disposed;

    public IStockRepository Stocks => serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<StockRepository>();

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