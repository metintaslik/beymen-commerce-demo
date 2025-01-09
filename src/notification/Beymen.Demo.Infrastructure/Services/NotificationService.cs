using Beymen.Demo.Application.DTOs;
using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Infrastructure.Factories;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Infrastructure.Services;

public class NotificationService(
    NotificationProviderFactory providerFactory,
    ILogger<NotificationService> logger,
    INotificationRepository notificationRepository) : INotificationService
{
    private readonly NotificationProviderFactory _providerFactory = providerFactory;
    private readonly ILogger<NotificationService> _logger = logger;
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task SendNotificationAsync(NotificationDto dto, CancellationToken cancellationToken = default)
    {
        var notification = Notification.Create(dto.Title, dto.Content, dto.Type, dto.Recipient);

        try
        {
            await _providerFactory.GetProvider(notification.Type).SendAsync(notification, cancellationToken);
            notification.MarkAsSent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification {Id}", notification.Id);
            notification.MarkAsFailed();
        }
        finally
        {
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
        }
    }
}
