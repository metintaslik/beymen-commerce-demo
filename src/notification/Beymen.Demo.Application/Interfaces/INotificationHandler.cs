using Beymen.Demo.Application.DTOs;
using Beymen.Demo.Domain.Entities;

namespace Beymen.Demo.Application.Interfaces;

public interface INotificationHandler
{
    Task HandleAsync(Notification notification, NotificationDto notificationDto, CancellationToken stoppingToken);
}