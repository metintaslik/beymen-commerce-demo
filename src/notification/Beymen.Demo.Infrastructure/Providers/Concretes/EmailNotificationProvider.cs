using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Infrastructure.Providers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beymen.Demo.Infrastructure.Providers.Concretes;

public class EmailNotificationProvider(ILogger<EmailNotificationProvider> logger) : INotificationProvider
{
    private readonly ILogger<EmailNotificationProvider> _logger = logger;

    public async Task SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Sending email notification to {notification.Recipient}\nTitle: {notification.Title}\nMessage: {notification.Content}");
        _logger.LogInformation("Sending email notification to {Id}", notification.Id);
        await Task.CompletedTask;
    }
}
