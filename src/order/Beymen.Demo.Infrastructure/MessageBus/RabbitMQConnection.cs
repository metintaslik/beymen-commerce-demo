using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Beymen.Demo.Infrastructure.MessageBus;

public class RabbitMQConnection : IRabbitMQConnection
{
    private readonly IConnection _connection;

    public IConnection? Connection => _connection;

    public bool IsConnected => _connection != null && _connection.IsOpen;


    public RabbitMQConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQConnection> logger)
    {
        try
        {
            _connection = connectionFactory.CreateConnectionAsync().Result;
            logger.LogDebug("RabbitMQ connection initialized.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RabbitMQ connection error.");
            throw;
        }
    }
}