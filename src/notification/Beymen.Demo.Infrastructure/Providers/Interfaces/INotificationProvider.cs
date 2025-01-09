using Beymen.Demo.Domain.Entities;

namespace Beymen.Demo.Infrastructure.Providers.Interfaces;

public interface INotificationProvider
{
    Task SendAsync(Notification notification, CancellationToken cancellationToken = default);
}
