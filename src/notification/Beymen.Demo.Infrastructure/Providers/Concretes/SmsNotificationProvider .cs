using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Infrastructure.Providers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Infrastructure.Providers.Concretes;

public class SmsNotificationProvider(ILogger<SmsNotificationProvider> logger) : INotificationProvider
{
    private readonly ILogger<SmsNotificationProvider> logger = logger;

    public async Task SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Sending SMS notification to {notification.Recipient}\nTitle: {notification.Title}\nMessage: {notification.Content}");
        logger.LogInformation("Sending SMS notification to {Id}", notification.Id);
        await Task.CompletedTask;
    }
}
