using Beymen.Demo.Application.DTOs;

namespace Beymen.Demo.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
}