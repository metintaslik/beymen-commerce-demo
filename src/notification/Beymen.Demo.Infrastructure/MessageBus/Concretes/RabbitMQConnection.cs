using Beymen.Demo.Infrastructure.MessageBus.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Beymen.Demo.Infrastructure.MessageBus.Concretes;

public class RabbitMQConnection : IRabbitMQConnection
{
    private readonly IConnection connection;
    private readonly ILogger<RabbitMQConnection> _logger;

    public RabbitMQConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQConnection> logger)
    {
        _logger = logger;

        try
        {
            connection = connectionFactory.CreateConnectionAsync().Result;
            _logger.LogDebug("RabbitMQ connection created.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ connection error.");
            throw new InvalidOperationException("Failed to create RabbitMQ connection.", ex);
        }
    }

    public IConnection Connection => connection;

    public bool IsConnected => Connection != null && Connection.IsOpen;
}