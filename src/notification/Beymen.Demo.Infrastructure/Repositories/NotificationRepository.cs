using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Domain.Enums;
using Beymen.Demo.Infrastructure.Persistance.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Beymen.Demo.Infrastructure.Repositories;

public class NotificationRepository(NotificationDbContext context, ILogger<NotificationRepository> logger) : INotificationRepository
{
    private readonly NotificationDbContext db = context;
    private readonly ILogger<NotificationRepository> _logger = logger;

    public async Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default!)
    {
        try
        {
            var entityEntry = await db.AddAsync(notification, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return entityEntry.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a notification");
            throw new InvalidOperationException("An error occurred while adding a notification", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Notification not found");
                return;
            }

            entity.MarkAsDelete();
            db.Entry(entity).State = EntityState.Modified;
            db.Update(entity);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting a notification");
        }
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        try
        {
            return await db.Notifications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while finding a notification");
            return null;
        }
    }

    public async Task<List<Notification>> GetListAsync(Expression<Func<Notification, bool>>? expression = null)
    {
        try
        {
            return expression is null ?
                await db.Notifications.OrderBy(x => x.Id).AsNoTracking().ToListAsync() :
                await db.Notifications.Where(expression).OrderBy(x => x.Id).AsNoTracking().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting a notification");
            return [];
        }
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await db.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Notification not found");
                return;
            }

            db.Entry(notification).State = EntityState.Modified;
            db.Update(notification);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating a notification");
        }
    }

    public async Task<IReadOnlyList<Notification>> GetPendingsAsync(int batchSize, CancellationToken stoppingToken)
    {
        try
        {
            return await db.Notifications
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedAt)
                .Take(batchSize)
                .ToListAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting pending notifications");
            return [];
        }
    }
}