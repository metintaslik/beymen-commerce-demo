using Beymen.Demo.Application.DTOs;
using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Infrastructure.MessageBus.Handlers;

public class NotificationHandler(
    INotificationRepository notificationRepository,
    INotificationService notificationService,
    ILogger<NotificationHandler> logger) : INotificationHandler
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ILogger<NotificationHandler> _logger = logger;

    public async Task HandleAsync(Notification notification, NotificationDto notificationDto, CancellationToken stoppingToken)
    {
        Notification? entity = null;

        try
        {
            entity = await _notificationRepository.AddAsync(notification, stoppingToken);

            _logger.LogDebug("Notification with ID {Id} has been added to the database.", entity.Id);

            await _notificationService.SendNotificationAsync(notificationDto, stoppingToken);

            _logger.LogDebug("Notification with Recipient: {Recipient} has been sent.", entity.Recipient);

            entity.MarkAsSent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling the notification.");
            entity?.MarkAsFailed();
        }
        finally
        {
            if (entity is not null)
            {
                await _notificationRepository.UpdateAsync(entity, stoppingToken);
            }
        }

        await Task.CompletedTask;
    }
}