using Beymen.Demo.Domain.Enums;
using Beymen.Demo.Infrastructure.Providers.Concretes;
using Beymen.Demo.Infrastructure.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Beymen.Demo.Infrastructure.Factories;

public class NotificationProviderFactory(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public INotificationProvider GetProvider(NotificationType type)
    {
        return type switch
        {
            NotificationType.Email => _serviceProvider.GetRequiredService<EmailNotificationProvider>(),
            NotificationType.SMS => _serviceProvider.GetRequiredService<SmsNotificationProvider>(),
            //NotificationType.Push => _serviceProvider.GetRequiredService<PushNotificationProvider>(),
            _ => throw new NotSupportedException($"Notification type {type} is not supported.")
        };
    }
}
