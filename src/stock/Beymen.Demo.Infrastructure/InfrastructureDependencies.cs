using Beymen.Demo.Application;
using Beymen.Demo.Domain.Interfaces;
using Beymen.Demo.Domain.Settings;
using Beymen.Demo.Infrastructure.MessageBus;
using Beymen.Demo.Infrastructure.Persistance;
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
        services.AddApplicationServices();

        services.AddDbContext<StockDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddRabbitMQServices(configuration);

        return services;
    }

    public static IServiceCollection AddRabbitMQServices(this IServiceCollection services, IConfiguration configuration)
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
                Port = settings.Port,
            };

            return new RabbitMQConnection(factory, logger);
        });

        services.AddHostedService<RabbitMQConsumerService>();

        return services;
    }
}