using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Settings;
using Beymen.Demo.Infrastructure.Factories;
using Beymen.Demo.Infrastructure.MessageBus.BackgroundServices;
using Beymen.Demo.Infrastructure.MessageBus.Concretes;
using Beymen.Demo.Infrastructure.MessageBus.Handlers;
using Beymen.Demo.Infrastructure.MessageBus.Interfaces;
using Beymen.Demo.Infrastructure.Persistance.Data;
using Beymen.Demo.Infrastructure.Providers.Concretes;
using Beymen.Demo.Infrastructure.Providers.Interfaces;
using Beymen.Demo.Infrastructure.Repositories;
using Beymen.Demo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Beymen.Demo.Infrastructure;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName))
            );

        services.AddSingleton<NotificationProviderFactory>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddTransient<INotificationProvider, EmailNotificationProvider>();
        services.AddTransient<INotificationProvider, SmsNotificationProvider>();
        // if other any notification sender types are added in the future

        services.AddScoped<INotificationRepository, NotificationRepository>();

        // if any services types are added in the future Application Layer
        //services.AddApplicationServices();

        services.AddRabbitMQServices(configuration);

        return services;
    }

    private static IServiceCollection AddRabbitMQServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQSettings>(configuration.GetRequiredSection("RabbitMQ"));

        services.AddSingleton<IRabbitMQConnection>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
            var logger = sp.GetRequiredService<ILogger<RabbitMQConnection>>();

            var factory = new ConnectionFactory
            {
                HostName = settings.HostName!,
                UserName = settings.UserName!,
                Password = settings.Password!,
                Port = settings.Port
            };

            return new RabbitMQConnection(factory, logger);
        });

        services.AddHostedService<RabbitMQConsumerService>();

        services.AddScoped<INotificationHandler, NotificationHandler>();

        return services;
    }
}