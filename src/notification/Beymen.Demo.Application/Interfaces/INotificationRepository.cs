using Beymen.Demo.Domain.Entities;
using System.Linq.Expressions;

namespace Beymen.Demo.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetListAsync(Expression<Func<Notification, bool>>? expression = null);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default!);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<Notification>> GetPendingsAsync(int batchSize, CancellationToken stoppingToken);
}